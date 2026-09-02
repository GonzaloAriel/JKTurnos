using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// M-08: Portal publico de reservas por subdominio. Operaciones SOLO publicas
    /// (an�nimas): resuelve el tenant desde el host, lista recursos y slots libres,
    /// y crea la reserva + el link de confirmacion por WhatsApp.
    ///
    /// REGLA DE SEGURIDAD CLAVE (decision TEAM LEAD 2026-09-01): el tenant se resuelve
    /// EXCLUSIVAMENTE por host de la peticion y NUNCA como mecanismo de autorizacion;
    /// se usa un JKContext nuevo por llamada (patron RecursoServicioService.Vincular),
    /// fuera de los repositorios BaseRepository/GlobalRepository que dependen del
    /// claim TenantId de un usuario autenticado. A un tenant no se le accede datos de
    /// otro; el scope siempre es el TenantId recien resuelto del host.
    /// </summary>
    public class PortalReservaService
    {
        /// <summary>
        /// Resuelve un tenant a partir del header Host de la peticion. Devuelve null
        /// si el subdominio no corresponde a ningun tenant VALIDO con portal habilitado.
        /// No es un gate de autorizacion; solo contexto de navegacion publica.
        /// </summary>
        public Tenant ResolveTenantByHost(string host)
        {
            var subdominio = ExtractSubdominio(host);

            // MODO DEV (reversible): si el host no trae subdominio (p.ej. localhost)
            // y hay un subdominio de desarrollo configurado (JKPortalHostDevSubdominio),
            // lo usa como fallback. En produccion (sin ese appSetting) queda intacto:
            // sin subdominio => no se resuelve tenant. Nunca aplica si el host si trae
            // subdominio, por lo que el portal real por subdominio no se ve afectado.
            if (string.IsNullOrWhiteSpace(subdominio))
            {
                var subdominioDev = (System.Configuration.ConfigurationManager
                    .AppSettings["JKPortalHostDevSubdominio"] ?? "").Trim();

                if (string.IsNullOrWhiteSpace(subdominioDev))
                    return null;

                subdominio = subdominioDev;
            }

            using (var db = new JKContext())
            {
                return db.Tenants
                    .FirstOrDefault(t => t.Subdominio == subdominio);
            }
        }

        /// <summary>
        /// True si el tenant existe, esta activo (no baja logica), no suspendido
        /// manualmente y tiene el portal publico habilitado (opt-in del admin).
        /// Determina si el portal renderiza el flujo de reserva o el 404 neutro.
        /// </summary>
        public bool PortalDisponible(Tenant tenant)
        {
            if (tenant == null || !tenant.IsActive || tenant.SuspendidoManual)
                return false;

            using (var db = new JKContext())
            {
                var config = db.ConfiguracionesTenant
                    .FirstOrDefault(c => c.TenantId == tenant.Id);
                return config != null && config.PortalHabilitado;
            }
        }

        public ConfiguracionTenant GetConfiguracion(int tenantId)
        {
            using (var db = new JKContext())
            {
                return db.ConfiguracionesTenant
                    .FirstOrDefault(c => c.TenantId == tenantId);
            }
        }

        public List<Recurso> GetRecursos(int tenantId)
        {
            using (var db = new JKContext())
            {
                return db.Recursos
                    .Where(r => r.TenantId == tenantId && r.IsActive)
                    .OrderBy(r => r.Nombre)
                    .ToList();
            }
        }

        /// <summary>
        /// Genera los slots libres de 30 min para un recurso en una fecha, dentro de
        /// una franja laboral configurable (inicio fin en minutos desde 00:00).
        /// Reutiliza la semantica de disponibilidad de TurnoService: un turno la
        /// ocupa si se solapa y no esta CANCELADO (D-042, el cancelado libera su slot).
        /// </summary>
        public List<string> GetSlotsLibres(int tenantId, int recursoId, DateTime fecha,
            int aperturaMinutos = 9 * 60, int cierreMinutos = 21 * 60, int duracionMinutos = 30)
        {
            var slots = new List<string>();

            using (var db = new JKContext())
            {
                var inicioDia = fecha.Date;
                var finDia = inicioDia.AddDays(1);

                var ocupados = db.Turnos
                    .Where(t => t.TenantId == tenantId
                                && t.RecursoId == recursoId
                                && t.FechaHoraInicio < finDia
                                && t.FechaHoraFin > inicioDia)
                    .Where(t => t.EstadoTurno == null
                                || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado)
                    .ToList();

                for (var min = aperturaMinutos; min + duracionMinutos <= cierreMinutos; min += duracionMinutos)
                {
                    var inicio = inicioDia.AddMinutes(min);
                    var fin = inicio.AddMinutes(duracionMinutos);

                    if (inicio < DateTime.Now)
                        continue;

                    var libre = !ocupados.Any(o =>
                        o.FechaHoraInicio < fin && o.FechaHoraFin > inicio);

                    if (libre)
                        slots.Add(inicio.ToString("HH:mm"));
                }

                return slots;
            }
        }

        /// <summary>
        /// Crea o recupera el cliente del tenant por telefono (normalizado a digitos).
        /// Si no existe, crea uno Pendiente sin estado (Se crea como cliente normal).
        /// </summary>
        public Cliente ObtenerOCrearCliente(int tenantId, string nombre, string telefono, string email)
        {
            var tel = NormalizarTelefono(telefono);

            using (var db = new JKContext())
            {
                // Busca por telefono normalizado para no duplicar clientes que reservan varias veces.
                if (!string.IsNullOrEmpty(tel))
                {
                    var todos = db.Clientes
                        .Where(c => c.TenantId == tenantId && c.IsActive)
                        .ToList();

                    var existente = todos.FirstOrDefault(c =>
                        NormalizarTelefono(c.Telefono) == tel);

                    if (existente != null)
                        return existente;
                }

                var cliente = new Cliente
                {
                    TenantId = tenantId,
                    Nombre = (nombre ?? "").Trim(),
                    Telefono = telefono,
                    Email = email,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                db.Clientes.Add(cliente);
                db.SaveChanges();
                return cliente;
            }
        }

        /// <summary>
        /// Crea la reserva (turno Pendiente) para el tenant publico. Valida que el
        /// recurso pertenezca al tenant, calcula el fin segun la duracion del servicio
        /// (o 30 min por defecto), valida disponibilidad del slot y asigna el estado
        /// PENDIENTE del catalogo. Devuelve el turno creado. Audita (sin depender del
        /// claim, se pasa el tenantId explicitamente).
        /// </summary>
        public Turno CrearReserva(int tenantId, int recursoId, DateTime fechaHoraInicio,
            string nombre, string telefono, string email, string observaciones)
        {
            using (var db = new JKContext())
            {
                // Validar recurso dentro del tenant
                var recurso = db.Recursos
                    .FirstOrDefault(r => r.Id == recursoId && r.TenantId == tenantId && r.IsActive);

                if (recurso == null)
                    throw new InvalidOperationException("El recurso seleccionado no es valido.");

                if (fechaHoraInicio < DateTime.Now)
                    throw new InvalidOperationException(
                        "La fecha y hora de la reserva no puede ser anterior a la actual.");

                // Estado PENDIENTE (catalogo fijo). Si no existe, se determina el fallback.
                var estadoPendiente = db.EstadosTurno
                    .FirstOrDefault(e => e.TenantId == tenantId && e.Codigo == EstadosTurnoBase.Pendiente);

                if (estadoPendiente == null)
                    throw new InvalidOperationException(
                        "El negocio no tiene configurado el estado Pendiente. Contacte al administrador.");

                var cliente = ObtenerOCrearCliente(tenantId, nombre, telefono, email);

                var fin = fechaHoraInicio.AddMinutes(30); // flujo minimo: sin servicio => 30 min

                // Disponibilidad (misma semantica que TurnoService): el cancelado libera el slot.
                var ocupado = db.Turnos.Any(t =>
                    t.TenantId == tenantId
                    && t.RecursoId == recursoId
                    && t.FechaHoraInicio < fin
                    && t.FechaHoraFin > fechaHoraInicio
                    && (t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado));

                if (ocupado)
                    throw new InvalidOperationException(
                        "El horario seleccionado ya no esta disponible. Elegi otro.");

                var turno = new Turno
                {
                    TenantId = tenantId,
                    ClienteId = cliente.Id,
                    RecursoId = recursoId,
                    EstadoTurnoId = estadoPendiente.Id,
                    FechaHoraInicio = fechaHoraInicio,
                    FechaHoraFin = fin,
                    Observaciones = observaciones,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                db.Turnos.Add(turno);
                db.SaveChanges();

                // Auditoria publica: el tenant va explicito (sin claim autenticado).
                new AuditService().RegistrarPublico(
                    tenantId,
                    AuditService.A_Crear,
                    AuditService.E_Turno,
                    "Reserva online (portal): " + cliente.Nombre + " " +
                    fechaHoraInicio.ToString("dd/MM/yyyy HH:mm"),
                    turno.Id);

                return turno;
            }
        }

        /// <summary>
        /// Genera el link wa.me del CLIENTE con el mensaje de confirmacion precargado
        /// (misma logica que el boton "Recordar por WhatsApp" del drawer). Reutiliza
        /// NotificacionService.GenerarLinkWhatsApp, que lee la plantilla del negocio.
        /// FLUJO ANONIMO (M-08): la config se carga por tenantId explicito (sin claim),
        /// porque el portal no corre con sesion autenticada.
        /// Requiere cargar Cliente/Servicio para el nombre y el texto.
        /// </summary>
        public string GenerarLinkConfirmacion(int tenantId, int turnoId)
        {
            using (var db = new JKContext())
            {
                var turno = db.Turnos
                    .Include("Cliente")
                    .Include("Servicio")
                    .FirstOrDefault(t => t.Id == turnoId);

                if (turno == null)
                    return null;

                var config = GetConfiguracion(tenantId);
                return new NotificacionService().GenerarLinkWhatsApp(turno, config);
            }
        }

        /// <summary>
        /// Extrae el subdominio (primer label) del host, validando contra un dominio
        /// raiz configurado (Web.config appSetting JKPortalDominioRaiz). Si el host es
        /// el dominio raiz mismo (o localhost), el subdominio es null. Con supports
        /// de X-Forwarded-Host por los proxies IIS.
        /// </summary>
        private string ExtractSubdominio(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return null;

            // Quitar puerto (por si llega con :44332).
            var sinPuerto = host.Trim().ToLower();
            var idx = sinPuerto.IndexOf(':');
            if (idx > 0)
                sinPuerto = sinPuerto.Substring(0, idx);

            if (sinPuerto.Length == 0)
                return null;

            // Solo hostnames DNS: descarta IPs y host raros.
            var hostType = Uri.CheckHostName(sinPuerto);
            if (hostType != UriHostNameType.Dns)
                return null;

            var labels = sinPuerto.Split('.');

            // Sin subdominio: hostname simple (localhost, dominio raiz, www) o inversa IP.
            if (labels.Length < 2 || labels[0] == "www")
                return null;

            // El subdominio es el primer label que NO es el dominio raiz.
            var dominioRaiz = (System.Configuration.ConfigurationManager
                .AppSettings["JKPortalDominioRaiz"] ?? "").Trim().ToLower();

            if (!string.IsNullOrEmpty(dominioRaiz)
                && labels.Length == 2 && labels[1] == dominioRaiz)
                return null;

            return labels[0];
        }

        private static string NormalizarTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return null;

            var digits = Regex.Replace(telefono, @"[^\d]", "");
            return digits.Length == 0 ? null : digits;
        }
    }
}