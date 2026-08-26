using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    /// <summary>
    /// D-037 / AG-3: pagos de suscripcion de plataforma. Entidad de plataforma:
    /// acceso exclusivo via GlobalRepository (D-022). Registrar = confirmado
    /// (sin estados pendientes). Registro financiero: sin edicion ni baja en v1.
    /// </summary>
    public class PagoSuscripcionService
    {
        private readonly GlobalRepository<PagoSuscripcion> _pagoRepo;
        private readonly GlobalRepository<Tenant> _tenantRepo;

        public PagoSuscripcionService()
        {
            _pagoRepo = new GlobalRepository<PagoSuscripcion>();
            _tenantRepo = new GlobalRepository<Tenant>();
        }

        public List<PagoSuscripcion> GetAll()
        {
            return _pagoRepo.GetAll()
                            .OrderByDescending(x => x.Fecha)
                            .ThenByDescending(x => x.Id)
                            .ToList();
        }

        public PagoSuscripcion GetById(int id)
        {
            return _pagoRepo.GetById(id);
        }

        public List<PagoSuscripcion> GetByTenant(int tenantId)
        {
            return _pagoRepo.GetAll()
                            .Where(x => x.TenantId == tenantId)
                            .OrderByDescending(x => x.Fecha)
                            .ThenByDescending(x => x.Id)
                            .ToList();
        }

        /// <summary>
        /// D-037 / AG-0 punto 5 / AG-4: registra un pago confirmado y actualiza la
        /// suscripcion del tenant en UNA transaccion (pago + vencimiento, todo o nada).
        /// Recalculo de vigencia (PlanesSuscripcion.CalcularNuevoVencimiento):
        /// - Si Vencimiento >= fecha del pago (vigente): extiende desde el vencimiento
        ///   vigente para que el tenant no pierda dias (renovacion anticipada).
        /// - Si no (vencido/en gracia/sin plan): parte de la fecha del pago.
        /// FechaInicioPeriodo = fecha del pago; PlanCodigo del tenant = plan de este
        /// pago (el ultimo pago manda). No toca SuspendidoManual ni IsActive.
        /// UsuarioId = AdminGlobal que registra (claim del usuario autenticado).
        /// Devuelve el pago creado.
        /// </summary>
        public PagoSuscripcion Registrar(int tenantId, string planCodigo, decimal monto, DateTime fecha, string observacion, int usuarioId)
        {
            if (!PlanesSuscripcion.Todos.Any(p => p.Codigo == planCodigo))
                throw new InvalidOperationException("Plan de suscripcion desconocido: " + planCodigo);

            if (monto < 0m)
                throw new InvalidOperationException("El monto no puede ser negativo.");

            if (!string.IsNullOrEmpty(observacion) && observacion.Trim().Length > 500)
                throw new InvalidOperationException("La observacion no puede superar 500 caracteres.");

            using (var context = new JKContext())
            using (var transaccion = context.Database.BeginTransaction())
            {
                var pagoRepo = new GlobalRepository<PagoSuscripcion>(context);
                var tenantRepo = new GlobalRepository<Tenant>(context);

                var tenant = tenantRepo.GetById(tenantId);

                if (tenant == null)
                    throw new InvalidOperationException("El tenant no existe.");

                var pago = new PagoSuscripcion
                {
                    TenantId = tenantId,
                    PlanCodigo = planCodigo,
                    Monto = monto,
                    Fecha = fecha,
                    Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(),
                    UsuarioId = usuarioId
                };

                pagoRepo.Add(pago);

                tenant.PlanCodigo = planCodigo;
                tenant.FechaInicioPeriodo = fecha.Date;
                tenant.FechaVencimiento = PlanesSuscripcion.CalcularNuevoVencimiento(tenant.FechaVencimiento, fecha, planCodigo);

                tenantRepo.Update(tenant);

                transaccion.Commit();

                return pago;
            }
        }
    }
}
