using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// Datos del negocio (ConfiguracionTenant) + CRUD de estados de turno del tenant.
    /// Reglas en el servicio: los estados se crean por tenant; el nombre es obligatorio;
    /// el color tiene un default neutral si no se indica.
    /// </summary>
    public class ConfiguracionService
    {
        private readonly BaseRepository<ConfiguracionTenant> _configRepo;
        private readonly BaseRepository<EstadoTurno> _estadoRepo;

        public ConfiguracionService()
        {
            _configRepo = new BaseRepository<ConfiguracionTenant>();
            _estadoRepo = new BaseRepository<EstadoTurno>();
        }

        public ConfiguracionTenant GetConfiguracion()
        {
            return _configRepo.GetAll().FirstOrDefault();
        }

        // Datos del negocio (forma DatosNegocio). No toca las notificaciones:
        // se editan en su propia seccion (NotificacionesEditar).
        public void UpdateDatosNegocio(ConfiguracionTenant model)
        {
            model.NombreComercial = (model.NombreComercial ?? "").Trim();

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
                AuditService.Campo("AvatarUrl", actual.AvatarUrl, model.AvatarUrl));

            actual.NombreComercial = model.NombreComercial;
            actual.Telefono = model.Telefono;
            actual.Email = model.Email;
            actual.Direccion = model.Direccion;
            actual.LogoUrl = model.LogoUrl;
            actual.AvatarUrl = model.AvatarUrl;
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

        public List<EstadoTurno> GetEstados()
        {
            return _estadoRepo.GetAll().OrderBy(x => x.Nombre).ToList();
        }

        public EstadoTurno GetEstado(int id)
        {
            return _estadoRepo.GetById(id);
        }

        public void CrearEstado(string nombre, string color)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del estado es obligatorio.");

            var estado = new EstadoTurno
            {
                Nombre = nombre.Trim(),
                Color = NormalizarColor(color)
            };

            _estadoRepo.Add(estado);

            new AuditService().Registrar(
                AuditService.A_Crear,
                AuditService.E_EstadoTurno,
                "Creo el estado de turno " + estado.Nombre,
                estado.Id);
        }

        public void ActualizarEstado(int id, string nombre, string color)
        {
            var actual = _estadoRepo.GetById(id);

            if (actual == null)
                throw new InvalidOperationException("Estado no encontrado.");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del estado es obligatorio.");

            var cambios = AuditService.FormatearCambios(
                AuditService.Campo("Nombre", actual.Nombre, nombre.Trim()),
                AuditService.Campo("Color", actual.Color, NormalizarColor(color)));

            actual.Nombre = nombre.Trim();
            actual.Color = NormalizarColor(color);
            _estadoRepo.Update(actual);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_EstadoTurno,
                "Edito el estado de turno " + actual.Nombre,
                id,
                cambios);
        }

        public void EliminarEstado(int id)
        {
            var actual = _estadoRepo.GetById(id);

            _estadoRepo.Delete(id);

            new AuditService().Registrar(
                AuditService.A_Eliminar,
                AuditService.E_EstadoTurno,
                actual != null ? "Elimino el estado de turno " + actual.Nombre : "Elimino el estado de turno #" + id,
                id);
        }

        private static string NormalizarColor(string color)
        {
            return string.IsNullOrWhiteSpace(color) ? "#6c757d" : color.Trim();
        }
    }
}