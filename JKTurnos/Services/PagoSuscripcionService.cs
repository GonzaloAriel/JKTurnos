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
        /// D-037 / AG-0 punto 5 / AG-4 / C4: registra un pago confirmado y actualiza la
        /// suscripcion del tenant en UNA transaccion (pago + vencimiento, todo o nada).
        /// C4 - idempotencia: si ya existe un PagoSuscripcion con la misma IdempotencyKey
        /// (reenvio del mismo submit), devuelve ese pago sin duplicar ni re-aplicar el
        /// recalculo de vencimiento (el submit original ya lo hizo). No hay indice unico
        /// (rechazaria historicos con NULL duplicados en este SQL Server); la verificacion
        /// transaccional por clave es la fuente de idempotencia.
        /// Recalculo de vigencia (PlanesSuscripcion.CalcularNuevoVencimiento):
        /// - Si Vencimiento >= fecha del pago (vigente): extiende desde el vencimiento
        ///   vigente para que el tenant no pierda dias (renovacion anticipada).
        /// - Si no (vencido/en gracia/sin plan): parte de la fecha del pago.
        /// FechaInicioPeriodo = fecha del pago; PlanCodigo del tenant = plan de este
        /// pago (el ultimo pago manda). No toca SuspendidoManual ni IsActive.
        /// UsuarioId = AdminGlobal que registra (claim del usuario autenticado).
        /// Devuelve el pago creado (o el pre-existente si la clave se repite).
        /// </summary>
        public PagoSuscripcion Registrar(int tenantId, string planCodigo, decimal monto, DateTime fecha, string observacion, int usuarioId, string idempotencyKey)
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

                // C4: idempotencia. Si el mismo submit (misma clave) ya genero un pago,
                // devolvemos ese pago sin duplicar ni re-aplicar el recalculo.
                if (!string.IsNullOrEmpty(idempotencyKey))
                {
                    var existente = pagoRepo.GetAll()
                                            .FirstOrDefault(x => x.IdempotencyKey == idempotencyKey);
                    if (existente != null)
                    {
                        transaccion.Commit();
                        return existente;
                    }
                }

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
                    UsuarioId = usuarioId,
                    IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim()
                };

                pagoRepo.Add(pago);

                tenant.PlanCodigo = planCodigo;
                tenant.FechaInicioPeriodo = fecha.Date;
                tenant.FechaVencimiento = PlanesSuscripcion.CalcularNuevoVencimiento(tenant.FechaVencimiento, fecha, planCodigo);

                tenantRepo.Update(tenant);

                // C6: notificar al tenant en su feed in-app (NotificacionSistema, por
                // tenant). Se inserta con TenantId explicito (el AdminGlobal corre con
                // TenantId=0; BaseRepository.Add lo forzaria a 0 y el tenant no lo veria).
                // Dentro del MISMO contexto/transaccion: pago + suscripcion + notificacion,
                // todo o nada. Sin navegacion (EntidadTipo=null): el feed del tenant no
                // tiene pagina operativa para suscripciones.
                var notifRepo = new GlobalRepository<NotificacionSistema>(context);
                notifRepo.Add(new NotificacionSistema
                {
                    TenantId = tenantId,
                    Tipo = NotificacionSistemaService.TipoPagoRegistrado,
                    Titulo = "Pago de suscripcion",
                    Mensaje = "Pago registrado por $" + monto.ToString("0.##") + " (plan " + planCodigo + ").",
                    EntidadTipo = null,
                    EntidadId = null,
                    Fecha = DateTime.Now,
                    Leida = false
                });

                transaccion.Commit();

                return pago;
            }
        }
    }
}
