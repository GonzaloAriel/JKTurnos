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

        // D-046 / M-01 (Concepto B): minutos de la Franja Formal (HoraCierre - HoraApertura).
        // Informativo y no bloqueante. Si no esta configurada (o es invalida) cae a
        // 08:00 - 19:30 = 690 min (valor historico del KPI de ocupacion).
        public int GetMinutosJornada()
        {
            var config = GetConfiguracion();

            if (config == null || !config.HoraApertura.HasValue || !config.HoraCierre.HasValue)
                return 690;

            var dif = config.HoraCierre.Value - config.HoraApertura.Value;

            if (dif <= TimeSpan.Zero)
                return 690;

            return (int)dif.TotalMinutes;
        }

        // Dias laborables: parsea el string "1,2,3,4,5,6" a un HashSet<DayOfWeek>.
        // Si es null o vacio, retorna Lun-Sab como default.
        public HashSet<DayOfWeek> GetDiasLaborales()
        {
            var config = GetConfiguracion();
            return ParsearDiasLaborales(config?.DiasLaborales);
        }

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
                var trimmed = parte.Trim();
                int num;
                if (int.TryParse(trimmed, out num) && num >= 1 && num <= 7)
                {
                    // 1=Lun=Monday..7=Dom=Sunday. Mapeo: (num % 7) + 1 == DayOfWeek value?
                    // DayOfWeek: 0=Sun, 1=Mon, 2=Tue, 3=Wed, 4=Thu, 5=Fri, 6=Sat
                    // Nuestro:  1=Mon, 2=Tue, 3=Wed, 4=Thu, 5=Fri, 6=Sat, 7=Sun
                    // Mapeo: nuestro 1 -> C# 1 (Mon), 7 -> 0 (Sun)
                    var dow = num == 7 ? DayOfWeek.Sunday : (DayOfWeek)num;
                    resultado.Add(dow);
                }
            }

            return resultado.Count > 0 ? resultado : defaultDias;
        }

        private string FormatearDiasLaborales(HashSet<DayOfWeek> dias)
        {
            if (dias == null || dias.Count == 0)
                return "1,2,3,4,5,6";

            // Orden: Lun(1)..Dom(7)
            var partes = new List<string>();
            for (int i = 1; i <= 7; i++)
            {
                var dow = i == 7 ? DayOfWeek.Sunday : (DayOfWeek)i;
                if (dias.Contains(dow))
                    partes.Add(i.ToString());
            }

            return partes.Count > 0 ? string.Join(",", partes) : "1,2,3,4,5,6";
        }

        // Datos del negocio (forma DatosNegocio). No toca las notificaciones:
        // se editan en su propia seccion (NotificacionesEditar).
        public void UpdateDatosNegocio(ConfiguracionTenant model)
        {
            model.NombreComercial = (model.NombreComercial ?? "").Trim();

            // D-046 / M-01 (Concepto B): normaliza la Franja Formal. Si cae o esta
            // incompleta se deja null (la logica usara 08:00 - 19:30 por defecto).
            if (model.HoraApertura.HasValue && model.HoraCierre.HasValue)
            {
                if (model.HoraCierre.Value <= model.HoraApertura.Value)
                {
                    model.HoraApertura = null;
                    model.HoraCierre = null;
                }
            }
            else
            {
                model.HoraApertura = null;
                model.HoraCierre = null;
            }

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
                AuditService.Campo("HoraApertura", actual.HoraApertura?.ToString(@"hh\:mm"), model.HoraApertura?.ToString(@"hh\:mm")),
                AuditService.Campo("HoraCierre", actual.HoraCierre?.ToString(@"hh\:mm"), model.HoraCierre?.ToString(@"hh\:mm")),
                AuditService.Campo("DiasLaborales", actual.DiasLaborales, model.DiasLaborales));

            actual.NombreComercial = model.NombreComercial;
            actual.Telefono = model.Telefono;
            actual.Email = model.Email;
            actual.Direccion = model.Direccion;
            actual.LogoUrl = model.LogoUrl;
            actual.AvatarUrl = model.AvatarUrl;
            actual.HoraApertura = model.HoraApertura;
            actual.HoraCierre = model.HoraCierre;
            actual.DiasLaborales = model.DiasLaborales;
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