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

                // 5. Armar viewmodel publico.
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
                    // Posta 4: disponibilidad real solo si el tenant la habilita.
                    Disponibilidad = vidriera.MostrarDisponibilidad
                        ? GetAvailableSlotsInContext(ctx, tenantId)
                        : new List<VidrieraDisponibilidadVM>()
                };
            }
        }

        /// <summary>
        /// Posta 4 / diseno seccion 13: proximos slots libres de HOY, por recurso.
        /// Jornada fija 08:00-19:30 (D-012, hasta M-01); franjas de 30 min.
        /// Regla de ocupacion IDENTICA a TurnoService.ValidarDisponibilidad: una
        /// franja esta ocupada si se solapa con un turno del recurso en estado NO
        /// Cancelado (D-040/D-042: el Cancelado libera su slot). Aislamiento por
        /// TenantId (solo consulta del mismo tenant del slug).
        /// </summary>
        public List<VidrieraDisponibilidadVM> GetAvailableSlots(int tenantId)
        {
            using (var ctx = new JKContext())
            {
                return GetAvailableSlotsInContext(ctx, tenantId);
            }
        }

        private List<VidrieraDisponibilidadVM> GetAvailableSlotsInContext(JKContext ctx, int tenantId)
        {
            var hoy = DateTime.Today;
            var jornadaInicio = hoy.AddHours(8);          // 08:00
            var jornadaFin = hoy.AddHours(19).AddMinutes(30); // 19:30

            var recursos = ctx.Recursos
                .Where(r => r.TenantId == tenantId && r.IsActive)
                .OrderBy(r => r.Nombre)
                .ToList();

            if (recursos.Count == 0)
                return new List<VidrieraDisponibilidadVM>();

            // Turnos del dia del tenant (con su estado para resolver el codigo).
            var turnos = ctx.Turnos
                .Where(t => t.TenantId == tenantId
                            && t.FechaHoraInicio < jornadaFin
                            && t.FechaHoraFin > jornadaInicio)
                .Include(t => t.EstadoTurno)
                .ToList();

            // Franja inicial: desde ahora (ceil al multiplo de 30) o desde la
            // apertura si es mas temprano; nunca mostrar horas ya pasadas.
            var ahora = DateTime.Now;
            int proxFranja = ((ahora.Hour * 60 + ahora.Minute + 29) / 30) * 30;
            var inicio = hoy.AddMinutes(proxFranja);
            if (inicio < jornadaInicio)
                inicio = jornadaInicio;

            var resultado = new List<VidrieraDisponibilidadVM>();

            foreach (var rec in recursos)
            {
                var turnosRec = turnos
                    .Where(t => t.RecursoId == rec.Id && (t.EstadoTurno == null || t.EstadoTurno.Codigo != EstadosTurnoBase.Cancelado))
                    .ToList();

                var libres = new List<string>();
                var franjaStart = inicio;
                while (franjaStart < jornadaFin)
                {
                    var franjaEnd = franjaStart.AddMinutes(30);
                    bool ocupada = turnosRec.Any(t => t.FechaHoraInicio < franjaEnd && t.FechaHoraFin > franjaStart);
                    if (!ocupada)
                        libres.Add(franjaStart.ToString("HH:mm"));
                    franjaStart = franjaEnd;
                }

                resultado.Add(new VidrieraDisponibilidadVM
                {
                    RecursoNombre = rec.Nombre,
                    SlotsLibres = libres
                });
            }

            return resultado;
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
    }
}
