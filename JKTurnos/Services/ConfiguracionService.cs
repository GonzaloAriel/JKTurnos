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
                return;
            }

            actual.NombreComercial = model.NombreComercial;
            actual.Telefono = model.Telefono;
            actual.Email = model.Email;
            actual.Direccion = model.Direccion;
            actual.LogoUrl = model.LogoUrl;
            _configRepo.Update(actual);
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

            _estadoRepo.Add(new EstadoTurno
            {
                Nombre = nombre.Trim(),
                Color = NormalizarColor(color)
            });
        }

        public void ActualizarEstado(int id, string nombre, string color)
        {
            var actual = _estadoRepo.GetById(id);

            if (actual == null)
                throw new InvalidOperationException("Estado no encontrado.");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del estado es obligatorio.");

            actual.Nombre = nombre.Trim();
            actual.Color = NormalizarColor(color);
            _estadoRepo.Update(actual);
        }

        public void EliminarEstado(int id)
        {
            _estadoRepo.Delete(id);
        }

        private static string NormalizarColor(string color)
        {
            return string.IsNullOrWhiteSpace(color) ? "#6c757d" : color.Trim();
        }
    }
}