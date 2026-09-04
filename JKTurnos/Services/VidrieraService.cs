using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Services
{
    /// <summary>
    /// D-042 / Posta 3 (decision A): lectura PUBLICA y anonima de la Vidriera (M-13).
    /// Consulta JKContext directamente filtrado por TenantId (NO BaseRepository, que
    /// depende del claim TenantId; NO GlobalRepository, que es de plataforma). El
    /// TenantId se obtiene SOLO desde el slug; toda consulta posterior filtra por ese
    /// mismo TenantId. Nunca se acepta un TenantId del navegador.
    /// </summary>
    public class VidrieraService
    {
        /// <summary>
        /// Resuelve la vidriera publica por slug. Devuelve null si no existe, si
        /// no esta habilitada, si no esta activa o si el tenant no esta activo.
        /// Pura lectura; no modifica nada.
        /// </summary>
        public VidrieraPublicaViewModel GetVidrieraPublica(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            slug = slug.Trim().ToLower();

            using (var ctx = new JKContext())
            {
                // 1. Slug -> VidrieraPublica habilitada y activa + su TenantId.
                var vidriera = ctx.VidrieraPublicas
                    .FirstOrDefault(v => v.Slug == slug && v.IsActive && v.Habilitada);

                if (vidriera == null)
                    return null;

                // 2. El tenant debe estar activo (tenant inactivo -> no expone).
                var tenant = ctx.Tenants.FirstOrDefault(t => t.Id == vidriera.TenantId && t.IsActive);
                if (tenant == null)
                    return null;

                var tenantId = vidriera.TenantId; // UNICA fuente de TenantId desde el slug.

                // 3. Identidad (desde ConfiguracionTenant, no se duplica).
                var config = ctx.ConfiguracionesTenant
                    .FirstOrDefault(c => c.TenantId == tenantId && c.IsActive);

                // 4. Servicios activos del MISMO tenantId (contenido principal).
                var servicios = ctx.Servicios
                    .Where(s => s.TenantId == tenantId && s.IsActive)
                    .OrderBy(s => s.Nombre)
                    .ToList();

                // 5. Franja real de la jornada (M-01): HoraApertura/HoraCierre con
                // fallback 08:00-19:30 (valor historico D-012). Se muestra como el
                // "rango de horarios" y acota los slots libres de la semana.
                var apertura = config != null && config.HoraApertura.HasValue
                    ? config.HoraApertura.Value
                    : TimeSpan.FromHours(8);
                var cierre = config != null && config.HoraCierre.HasValue
                    ? config.HoraCierre.Value
                    : TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));

                if (cierre <= apertura)
                {
                    apertura = TimeSpan.FromHours(8);
                    cierre = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
                }

                // 6. Armar viewmodel publico.
                var disponibles = vidriera.MostrarDisponibilidad
                    ? AgruparEnPantallas(GetDisponibilidadSemanalInContext(ctx, tenantId, apertura, cierre))
                    : new List<VidrieraPantallaVM>();

                return new VidrieraPublicaViewModel
                {
                    Slug = vidriera.Slug,
                    NombreNegocio = config?.NombreComercial ?? tenant.Nombre,
                    // Logo/avatar del negocio: se prefiere el LogoUrl propio; si no
                    // hay logo, se usa el AvatarUrl (avatar precargado). La vista
                    // muestra la marca circular como fallback solo si ambos faltan.
                    // Misma precedencia que BaseController.ViewBag.ConfiguracionLogo.
                    LogoUrl = config != null
                        ? (!string.IsNullOrWhiteSpace(config.LogoUrl)
                            ? config.LogoUrl
                            : config.AvatarUrl)
                        : null,
                    Telefono = config?.Telefono,
                    Email = config?.Email,
                    Direccion = config?.Direccion,
                    JornadaDesde = apertura,
                    JornadaHasta = cierre,
                    MostrarPrecios = vidriera.MostrarPrecios,
                    MostrarDisponibilidad = vidriera.MostrarDisponibilidad,
                    MostrarServicios = vidriera.MostrarServicios,
                    MostrarOfertas = vidriera.MostrarOfertas,
                    MostrarMensaje = vidriera.MostrarMensaje,
                    Titulo = vidriera.Titulo,
                    Descripcion = vidriera.Descripcion,
                    MensajePrincipal = vidriera.MensajePrincipal,
                    OfertaTitulo = vidriera.OfertaTitulo,
                    OfertaDescripcion = vidriera.OfertaDescripcion,
                    Servicios = servicios.Select(s => new VidrieraServicioVM
                    {
                        Nombre = s.Nombre,
                        Descripcion = s.Descripcion,
                        Precio = s.Precio,
                        DuracionMinutos = s.DuracionMinutos,
                        MostrarPrecio = vidriera.MostrarPrecios
                    }).ToList(),
                    // D-047: disponibilidad real SEMANAL solo si el tenant la habilita.
                    Pantallas = disponibles
                };
            }
        }

        /// <summary>
        /// D-047 / M-13 (Vidriera, posta semanal): MATRIZ de disponibilidad para los
        /// proximos 7 dias (hoy + 6), POR RECURSO, dentro de la jornada REAL del
        /// tenant (HoraApertura/HoraCierre, M-01; fallback 08:00-19:30). Franjas de
        /// 30 min. Regla de ocupacion igual a TurnoService.ValidarDisponibilidad: una
        /// franja esta ocupada si se solapa con un turno NO Cancelado (D-040) O con un
        /// BloqueoRecurso activo (D-046/M-01, mismo criterio Solapa). Aislamiento por
        /// TenantId (solo consulta el tenant del slug). Los dias/hojas pasadas no
        /// generan slots.
        /// </summary>
        public List<VidrieraPantallaVM> GetDisponibilidadSemanal(int tenantId)
        {
            using (var ctx = new JKContext())
            {
                var config = ctx.ConfiguracionesTenant
                    .FirstOrDefault(c => c.TenantId == tenantId && c.IsActive);
                var apertura = config != null && config.HoraApertura.HasValue
                    ? config.HoraApertura.Value
                    : TimeSpan.FromHours(8);
                var cierre = config != null && config.HoraCierre.HasValue
                    ? config.HoraCierre.Value
                    : TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
                if (cierre <= apertura)
                {
                    apertura = TimeSpan.FromHours(8);
                    cierre = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30));
                }

                var servicios = GetDisponibilidadSemanalInContext(ctx, tenantId, apertura, cierre);
                return AgruparEnPantallas(servicios);
            }
        }

        // D-048: parte la lista de servicios (filas) en pantallas de N servicios,
        // para que la pizarra nunca desborde el alto del monitor (sin scroll).
        // Si la lista ya fuera un solo servicio, una sola pantalla.
        public const int ServiciosPorPantalla = 5;

        private List<VidrieraPantallaVM> AgruparEnPantallas(List<VidrieraDisponibilidadSemanalVM> servicios)
        {
            var pantallas = new List<VidrieraPantallaVM>();
            if (servicios == null || servicios.Count == 0)
                return pantallas;

            int n = ServiciosPorPantalla;
            for (int i = 0; i < servicios.Count; i += n)
            {
                var grupo = servicios.Skip(i).Take(n).ToList();
                pantallas.Add(new VidrieraPantallaVM { Servicios = grupo });
            }
            return pantallas;
        }

        private List<VidrieraDisponibilidadSemanalVM> GetDisponibilidadSemanalInContext(
            JKContext ctx, int tenantId, TimeSpan apertura, TimeSpan cierre)
        {
            // Filas = SERVICIO. Los servicios agregan la disponibilidad de TODOS
            // sus recursos asociados (RecursoServicio): un slot es libre si algun
            // recurso del servicio lo tiene libre (union, sin repetir horas).
            var servicios = ctx.Servicios
                .Where(s => s.TenantId == tenantId && s.IsActive)
                .OrderBy(s => s.Nombre)
                .ToList();

            if (servicios.Count == 0)
                return new List<VidrieraDisponibilidadSemanalVM>();

            // Dias laborables del tenant: solo se renderizan estos dias en la vidriera.
            var configVidriera = ctx.ConfiguracionesTenant
                .FirstOrDefault(c => c.TenantId == tenantId && c.IsActive);
            var diasLaborales = ParsearDiasLaborales(configVidriera?.DiasLaborales);

            var recursosActivos = ctx.Recursos
                .Where(r => r.TenantId == tenantId && r.IsActive)
                .ToList();

            // servicioId -> recursos activos asociados.
            var recursosPorServicio = new Dictionary<int, List<Recurso>>();
            var relaciones = ctx.Set<RecursoServicio>()
                .Where(rs => rs.TenantId == tenantId)
                .ToList()
                .GroupBy(rs => rs.ServicioId);
            foreach (var g in relaciones)
            {
                var ids = g.Select(rs => rs.RecursoId).Distinct();
                recursosPorServicio[g.Key] = recursosActivos
                    .Where(r => ids.Contains(r.Id))
                    .ToList();
            }

            // Ventana: hoy + proximos 6 dias (extremo fin exclusivo).
            var hoy = DateTime.Today;
            var inicioVentana = hoy;
            var finVentana = hoy.AddDays(7);

            // Turnos del tenant dentro de la ventana (para resolver solapamiento por slot).
            var turnos = ctx.Turnos
                .Where(t => t.TenantId == tenantId
                            && t.FechaHoraInicio < finVentana
                            && t.FechaHoraFin > inicioVentana)
                .Include(t => t.EstadoTurno)
                .ToList();

            // Bloqueos del tenant dentro de la ventana (mismo criterio que la agenda).
            var bloqueos = ctx.BloqueosRecurso
                .Where(b => b.TenantId == tenantId
                            && b.FechaInicio < finVentana)
                .ToList()
                .Where(b => CoincideVentana(b, inicioVentana, finVentana))
                .ToList();

            // Por recurso: turnos NO Cancelados y bloqueos activos.
            var turnosPorRecurso = turnos
                .Where(t => t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado)
                .GroupBy(t => t.RecursoId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var bloqueosPorRecurso = bloqueos
                .GroupBy(b => b.RecursoId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dayNames = new[] { "LUN", "MAR", "MIE", "JUE", "VIE", "SAB", "DOM" };

            var resultado = new List<VidrieraDisponibilidadSemanalVM>();

            foreach (var servicio in servicios)
            {
                List<Recurso> recursosServicio;
                if (!recursosPorServicio.TryGetValue(servicio.Id, out recursosServicio) || recursosServicio.Count == 0)
                    recursosServicio = new List<Recurso>();

                var dias = new List<VidrieraDisponibilidadDiaVM>();

                for (int i = 0; i < 7; i++)
                {
                    var dia = hoy.AddDays(i);

                    // Solo procesar dias laborables del tenant.
                    if (!diasLaborales.Contains(dia.DayOfWeek))
                        continue;

                    bool esHoy = i == 0;

                    // Union de inicios libres (relativos al dia) de todos los recursos
                    // del servicio. Se ordena y se agrupan los consecutivos en rangos.
                    var libres = new SortedSet<TimeSpan>();

                    foreach (var rec in recursosServicio)
                    {
                        List<Turno> turnosRec;
                        turnosPorRecurso.TryGetValue(rec.Id, out turnosRec);
                        turnosRec = turnosRec ?? new List<Turno>();

                        List<BloqueoRecurso> bloqueosRec;
                        bloqueosPorRecurso.TryGetValue(rec.Id, out bloqueosRec);
                        bloqueosRec = bloqueosRec ?? new List<BloqueoRecurso>();

                        var franjaStart = dia.Add(apertura);
                        var franjaFin = dia.Add(cierre);

                        while (franjaStart < franjaFin)
                        {
                            var slotEnd = franjaStart.AddMinutes(30);
                            var slotRel = franjaStart - dia.Date;

                            // Slot pasado (hoy): no se ofrece.
                            bool pasado = slotEnd <= DateTime.Now;

                            if (!pasado)
                            {
                                bool ocupadaPorTurno = turnosRec.Any(t => t.FechaHoraInicio < slotEnd && t.FechaHoraFin > franjaStart);
                                bool ocupadaPorBloqueo = false;
                                foreach (var b in bloqueosRec)
                                {
                                    if (BloqueoSolapaSlot(b, dia, franjaStart, slotEnd))
                                    {
                                        ocupadaPorBloqueo = true;
                                        break;
                                    }
                                }

                                if (!ocupadaPorTurno && !ocupadaPorBloqueo)
                                    libres.Add(slotRel);
                            }

                            franjaStart = slotEnd;
                        }
                    }

                    // Convertir los inicios ordenados a rangos continuos ("09-11", "14").
                    var rangos = ConstruirRangos(libres);

                    dias.Add(new VidrieraDisponibilidadDiaVM
                    {
                        Fecha = dia,
                        NombreDia = dayNames[((int)dia.DayOfWeek + 6) % 7],
                        DiaDelMes = dia.Day,
                        EsHoy = esHoy,
                        CantidadSlots = libres.Count,
                        Rangos = rangos
                    });
                }

                resultado.Add(new VidrieraDisponibilidadSemanalVM
                {
                    ServicioNombre = servicio.Nombre,
                    Dias = dias
                });
            }

            return resultado;
        }

        // Convierte un conjunto ordenado de inicios (TimeSpan relativos al dia, paso
        // de 30 min) en horas individuales legibles para chips de la vidriera.
        // Cada hora de inicio de slot se muestra como un chip independiente.
        private List<string> ConstruirRangos(SortedSet<TimeSpan> libres)
        {
            var rangos = new List<string>();
            if (libres.Count == 0)
                return rangos;

            foreach (var s in libres)
            {
                // Mostrar la hora de inicio del slot como chip individual.
                rangos.Add(DateTime.Today.Add(s).ToString("HH:mm"));
            }

            return rangos;
        }

        // True si un BloqueoRecurso dado se solapa con la ventana [inicioVentana, finVentana).
        private bool CoincideVentana(BloqueoRecurso b, DateTime inicioVentana, DateTime finVentana)
        {
            switch (b.Tipo)
            {
                case TipoBloqueo.Unico:
                    var d = b.FechaInicio.Date;
                    return d >= inicioVentana && d < finVentana;

                case TipoBloqueo.RangoFechas:
                    var rIni = b.FechaInicio.Date;
                    var rFin = (b.FechaFin ?? b.FechaInicio).Date.AddDays(1);
                    return rIni < finVentana && rFin > inicioVentana;

                case TipoBloqueo.RecurrenteSemanal:
                default:
                    for (var f = inicioVentana; f < finVentana; f = f.AddDays(1))
                        if (f.DayOfWeek == b.FechaInicio.DayOfWeek)
                            return true;
                    return false;
            }
        }

        // True si el bloqueo cubre [dia + franjaStart, dia + franjaEnd) el dia dado.
        // Mismo criterio que BloqueoRecursoService.Solapa (misma fuente de verdad).
        private bool BloqueoSolapaSlot(BloqueoRecurso b, DateTime dia, DateTime franjaStart, DateTime franjaEnd)
        {
            bool coincideFecha;
            switch (b.Tipo)
            {
                case TipoBloqueo.Unico:
                    coincideFecha = b.FechaInicio.Date == dia.Date;
                    break;
                case TipoBloqueo.RangoFechas:
                    var diaFin = b.FechaFin.HasValue ? b.FechaFin.Value.Date : b.FechaInicio.Date;
                    coincideFecha = dia.Date >= b.FechaInicio.Date && dia.Date <= diaFin;
                    break;
                case TipoBloqueo.RecurrenteSemanal:
                default:
                    coincideFecha = dia.DayOfWeek == b.FechaInicio.DayOfWeek;
                    break;
            }

            if (!coincideFecha)
                return false;

            // Sin franja horaria (o invertida): cubre el dia completo.
            if (!b.HoraInicio.HasValue || !b.HoraFin.HasValue || b.HoraInicio.Value >= b.HoraFin.Value)
                return true;

            var bloqueoStart = dia.Date.Add(b.HoraInicio.Value);
            var bloqueoEnd = dia.Date.Add(b.HoraFin.Value);

            return franjaStart < bloqueoEnd && franjaEnd > bloqueoStart;
        }

        // ===== Posta 5: gestion (panel de administracion del tenant) =====

        /// <summary>
        /// Obtiene la vidriera del tenant para el panel de administracion.
        /// Devuelve null si el tenant todavia no tiene fila VidrieraPublica.
        /// Pura lectura (aislada por TenantId).
        /// </summary>
        public VidrieraAdminViewModel GetVidrieraAdmin(int tenantId)
        {
            using (var ctx = new JKContext())
            {
                var v = ctx.VidrieraPublicas.FirstOrDefault(x => x.TenantId == tenantId && x.IsActive);
                if (v == null)
                    return null;

                return MapAdmin(v);
            }
        }

        /// <summary>
        /// Crea o actualiza la vidriera del tenant desde el panel. Devuelve null si
        /// OK, o un mensaje de error (por ejemplo slug no disponible) que el
        /// controller muestra sin perder los datos del formulario. Normaliza el
        /// slug (minusculas, sin espacios) y valida su unicidad GLOBAL.
        /// </summary>
        public string GuardarVidriera(int tenantId, VidrieraAdminViewModel model)
        {
            model.Slug = NormalizarSlug(model.Slug);

            using (var ctx = new JKContext())
            {
                var actual = ctx.VidrieraPublicas.FirstOrDefault(x => x.TenantId == tenantId && x.IsActive);

                // El id propio se resuelve FUERA de la expresion LINQ. Acceder a
                // actual.Id dentro de la query con actual==null lanza TargetException
                // (EF6 no evalua la ternaria contra una entidad null capturada).
                int actualId = actual != null ? actual.Id : 0;

                // Unicidad global del slug (excluyendo la propia fila si existe).
                bool usado = ctx.VidrieraPublicas
                    .Any(x => x.Slug == model.Slug && x.IsActive && x.Id != actualId);
                if (usado)
                    return "Ese slug ya esta en uso. Elegi otro.";

                if (actual == null)
                {
                    actual = new VidrieraPublica
                    {
                        TenantId = tenantId,
                        Slug = model.Slug,
                        Habilitada = model.Habilitada,
                        Titulo = model.Titulo,
                        Descripcion = model.Descripcion,
                        MensajePrincipal = model.MensajePrincipal,
                        OfertaTitulo = model.OfertaTitulo,
                        OfertaDescripcion = model.OfertaDescripcion,
                        MostrarServicios = model.MostrarServicios,
                        MostrarPrecios = model.MostrarPrecios,
                        MostrarDisponibilidad = model.MostrarDisponibilidad,
                        MostrarOfertas = model.MostrarOfertas,
                        MostrarMensaje = model.MostrarMensaje,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    ctx.VidrieraPublicas.Add(actual);
                    ctx.SaveChanges();

                    new AuditService().Registrar(
                        AuditService.A_Crear,
                        AuditService.E_Vidriera,
                        "Creo la vidriera publica con slug " + model.Slug);
                }
                else
                {
                    var cambios = AuditService.FormatearCambios(
                        AuditService.Campo("Slug", actual.Slug, model.Slug),
                        AuditService.Campo("Habilitada", actual.Habilitada.ToString(), model.Habilitada.ToString()),
                        AuditService.Campo("Titulo", actual.Titulo, model.Titulo),
                        AuditService.Campo("Descripcion", actual.Descripcion, model.Descripcion),
                        AuditService.Campo("MensajePrincipal", actual.MensajePrincipal, model.MensajePrincipal),
                        AuditService.Campo("OfertaTitulo", actual.OfertaTitulo, model.OfertaTitulo),
                        AuditService.Campo("OfertaDescripcion", actual.OfertaDescripcion, model.OfertaDescripcion),
                        AuditService.Campo("MostrarServicios", actual.MostrarServicios.ToString(), model.MostrarServicios.ToString()),
                        AuditService.Campo("MostrarPrecios", actual.MostrarPrecios.ToString(), model.MostrarPrecios.ToString()),
                        AuditService.Campo("MostrarDisponibilidad", actual.MostrarDisponibilidad.ToString(), model.MostrarDisponibilidad.ToString()),
                        AuditService.Campo("MostrarOfertas", actual.MostrarOfertas.ToString(), model.MostrarOfertas.ToString()),
                        AuditService.Campo("MostrarMensaje", actual.MostrarMensaje.ToString(), model.MostrarMensaje.ToString()));

                    actual.Slug = model.Slug;
                    actual.Habilitada = model.Habilitada;
                    actual.Titulo = model.Titulo;
                    actual.Descripcion = model.Descripcion;
                    actual.MensajePrincipal = model.MensajePrincipal;
                    actual.OfertaTitulo = model.OfertaTitulo;
                    actual.OfertaDescripcion = model.OfertaDescripcion;
                    actual.MostrarServicios = model.MostrarServicios;
                    actual.MostrarPrecios = model.MostrarPrecios;
                    actual.MostrarDisponibilidad = model.MostrarDisponibilidad;
                    actual.MostrarOfertas = model.MostrarOfertas;
                    actual.MostrarMensaje = model.MostrarMensaje;
                    actual.UpdatedAt = DateTime.Now;
                    ctx.SaveChanges();

                    new AuditService().Registrar(
                        AuditService.A_Editar,
                        AuditService.E_Vidriera,
                        "Actualizo la vidriera publica",
                        null,
                        cambios);
                }
            }

            return null;
        }

        /// <summary>
        /// Normaliza el slug a formato de URL publica: minusculas, sin espacios
        /// (los espacios y conjuntos se convierten en guiones), sin caracteres
        /// invalidos duplicados.
        /// </summary>
        public string NormalizarSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return "";

            var s = slug.Trim().ToLowerInvariant();

            // Espacios y separadores -> guiones.
            s = System.Text.RegularExpressions.Regex.Replace(s, @"[\s_]+", "-");
            // Solo letras, numeros y guiones.
            s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\-]", "");
            // Sin guiones repetidos.
            s = System.Text.RegularExpressions.Regex.Replace(s, @"-{2,}", "-");
            // Sin guiones al inicio/fin.
            s = s.Trim('-');

            return s;
        }

        public string GetUrlPublica(string slug)
        {
            return "/vidriera/" + slug;
        }

        private VidrieraAdminViewModel MapAdmin(VidrieraPublica v)
        {
            return new VidrieraAdminViewModel
            {
                Slug = v.Slug,
                Habilitada = v.Habilitada,
                Titulo = v.Titulo,
                Descripcion = v.Descripcion,
                MensajePrincipal = v.MensajePrincipal,
                OfertaTitulo = v.OfertaTitulo,
                OfertaDescripcion = v.OfertaDescripcion,
                MostrarServicios = v.MostrarServicios,
                MostrarPrecios = v.MostrarPrecios,
                MostrarDisponibilidad = v.MostrarDisponibilidad,
                MostrarOfertas = v.MostrarOfertas,
                MostrarMensaje = v.MostrarMensaje,
                YaExiste = true,
                UrlPublica = GetUrlPublica(v.Slug)
            };
        }

        // Parsea "1,2,3,4,5,6" a HashSet<DayOfWeek>. Default Lun-Sab.
        private HashSet<DayOfWeek> ParsearDiasLaborales(string valor)
        {
            var defaultDias = new HashSet<DayOfWeek>
            {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
            };

            if (string.IsNullOrWhiteSpace(valor))
                return defaultDias;

            var resultado = new HashSet<DayOfWeek>();
            foreach (var parte in valor.Split(','))
            {
                int num;
                if (int.TryParse(parte.Trim(), out num) && num >= 1 && num <= 7)
                {
                    var dow = num == 7 ? DayOfWeek.Sunday : (DayOfWeek)num;
                    resultado.Add(dow);
                }
            }

            return resultado.Count > 0 ? resultado : defaultDias;
        }
    }
}
