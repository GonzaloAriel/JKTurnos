using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// Datos del negocio (ConfiguracionTenant): datos del negocio, plantillas de
    /// notificaciones (WhatsApp/Email) y recordatorios ocultos. Los estados de
    /// turno ya NO se configuran (D-040: catalogo FIJO del sistema).
    /// </summary>
    public class ConfiguracionService
    {
        private readonly BaseRepository<ConfiguracionTenant> _configRepo;

        public ConfiguracionService()
        {
            _configRepo = new BaseRepository<ConfiguracionTenant>();
        }

        public ConfiguracionTenant GetConfiguracion()
        {
            return _configRepo.GetAll().FirstOrDefault();
        }

        // D-050 / M-01 (Concepto B): minutos de atencion de UN DIA = suma de los
        // rangos del horario para ese dia (per-dia, soporta turno partido). El KPI
        // de ocupacion usa el denominador correcto del dia (un Sabado con franja
        // corta no penaliza). Informativo y no bloqueante.
        public int GetMinutosJornada(DateTime? fecha = null)
        {
            var config = GetConfiguracion();

            if (fecha.HasValue)
            {
                var horario = HorarioAtencionHelper.Resolver(config);
                var minutos = HorarioAtencionHelper.MinutosDelDia(horario, fecha.Value.DayOfWeek);
                if (minutos > 0)
                    return minutos;
            }

            return GetMinutosJornadaLegacy(config);
        }

        // Fallback historico: HoraCierre - HoraApertura (legacy). 08:00 - 19:30 = 690.
        private static int GetMinutosJornadaLegacy(ConfiguracionTenant config)
        {
            if (config != null && config.HoraApertura.HasValue && config.HoraCierre.HasValue)
            {
                var dif = config.HoraCierre.Value - config.HoraApertura.Value;
                if (dif > TimeSpan.Zero)
                    return (int)dif.TotalMinutes;
            }

            return 690;
        }

        // Dias en los que el negocio atiende: horario efectivo (JSON -> legacy ->
        // default Lun-Sab). Un dia "con horario" es aquel con al menos un rango.
        public HashSet<DayOfWeek> GetDiasLaborales()
        {
            var config = GetConfiguracion();
            var horario = HorarioAtencionHelper.Resolver(config);

            return horario.Count > 0
                ? new HashSet<DayOfWeek>(horario.Keys)
                : new HashSet<DayOfWeek>
                {
                    DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                    DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
                };
        }

        // Horario efectivo completo (JSON -> legacy -> default Lun-Sab 08:00-19:30).
        public List<HorarioAtencionDia> GetHorarioAtencion()
        {
            return HorarioAtencionHelper.ResolverLista(GetConfiguracion());
        }

        // Horario efectivo indexado por DayOfWeek (para la Agenda: celdas del
        // dia que caen fuera de los rangos se pintan como no clickeables).
        public Dictionary<DayOfWeek, List<RangoAtencion>> GetHorarioAtencionPorDia()
        {
            return HorarioAtencionHelper.Resolver(GetConfiguracion());
        }

        // Datos del negocio (forma DatosNegocio). No toca las notificaciones:
        // se editan en su propia seccion (NotificacionesEditar).
        public void UpdateDatosNegocio(ConfiguracionTenant model)
        {
            model.NombreComercial = (model.NombreComercial ?? "").Trim();

            // D-050 / M-01 (Concepto B): normaliza el horario por dia (JSON).
            // "" (o invalido) se guarda como null: los lectores degradan a legacy
            // (DiasLaborales+HoraApertura+HoraCierre) o a default Lun-Sab.
            var horarioNormalizado = HorarioAtencionHelper.Normalizar(model.HorarioAtencion);
            model.HorarioAtencion = string.IsNullOrWhiteSpace(horarioNormalizado) ? null : horarioNormalizado;

            var actual = _configRepo.GetAll().FirstOrDefault();

            if (actual == null)
            {
                _configRepo.Add(model);

                new AuditService().Registrar(
                    AuditService.A_Editar,
                    AuditService.E_Configuracion,
                    "Configuro los datos del negocio");

                return;
            }

            var cambios = AuditService.FormatearCambios(
                AuditService.Campo("NombreComercial", actual.NombreComercial, model.NombreComercial),
                AuditService.Campo("Telefono", actual.Telefono, model.Telefono),
                AuditService.Campo("Email", actual.Email, model.Email),
                AuditService.Campo("Direccion", actual.Direccion, model.Direccion),
                AuditService.Campo("LogoUrl", actual.LogoUrl, model.LogoUrl),
                AuditService.Campo("AvatarUrl", actual.AvatarUrl, model.AvatarUrl),
                AuditService.Campo("HorarioAtencion", actual.HorarioAtencion, model.HorarioAtencion));

            actual.NombreComercial = model.NombreComercial;
            actual.Telefono = model.Telefono;
            actual.Email = model.Email;
            actual.Direccion = model.Direccion;
            actual.LogoUrl = model.LogoUrl;
            actual.AvatarUrl = model.AvatarUrl;
            actual.HorarioAtencion = model.HorarioAtencion;
            _configRepo.Update(actual);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Configuracion,
                "Actualizo los datos del negocio",
                null,
                cambios);
        }

        // Plantilla del recordatorio por WhatsApp (forma NotificacionesWhatsApp).
        public void UpdateNotificacionWhatsApp(ConfiguracionTenant model)
        {
            var actual = _configRepo.GetAll().FirstOrDefault();

            if (actual == null)
            {
                _configRepo.Add(model);
                return;
            }

            actual.WhatsAppTemplate = model.WhatsAppTemplate;
            actual.WhatsAppTemplateDia = model.WhatsAppTemplateDia;
            _configRepo.Update(actual);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Configuracion,
                "Actualizo las plantillas de recordatorio por WhatsApp");
        }

        // Email SMTP (forma NotificacionesEmail). La password en blanco conserva
        // la existente (el form no re-renderiza el valor almacenado por seguridad).
        public void UpdateNotificacionEmail(ConfiguracionTenant model)
        {
            var actual = _configRepo.GetAll().FirstOrDefault();

            if (actual == null)
            {
                _configRepo.Add(model);
                return;
            }

            actual.EmailAsunto = model.EmailAsunto;
            actual.EmailTemplate = model.EmailTemplate;
            actual.EmailDesde = model.EmailDesde;
            actual.SmtpServidor = model.SmtpServidor;
            actual.SmtpPuerto = model.SmtpPuerto;
            actual.SmtpUsuario = model.SmtpUsuario;
            actual.SmtpHabilitado = model.SmtpHabilitado;

            if (!string.IsNullOrWhiteSpace(model.SmtpPassword))
                actual.SmtpPassword = model.SmtpPassword.Trim();

            _configRepo.Update(actual);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Configuracion,
                "Actualizo la configuracion de email");
        }

        // Claves de recordatorios ocultos con "Marcar todas leidas".
        public string GetRecordatoriosOcultos()
        {
            return _configRepo.GetAll().FirstOrDefault()?.RecordatoriosOcultos;
        }

        public void GuardarRecordatoriosOcultos(string claves)
        {
            var actual = _configRepo.GetAll().FirstOrDefault();
            if (actual == null)
                return;

            actual.RecordatoriosOcultos = claves;
            _configRepo.Update(actual);
        }
    }
}