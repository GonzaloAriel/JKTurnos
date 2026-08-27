using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Services
{
    /// <summary>
    /// D-022: dashboard de plataforma (AdminGlobal). Acceso exclusivo via
    /// GlobalRepository, que filtra solo por IsActive (sin TenantId). El conteo de
    /// pagos de plataforma hereda ese filtro (misma convencion de soft-delete).
    /// </summary>
    public class DashboardAdminGlobalService
    {
        private readonly GlobalRepository<Tenant> _tenantRepo;
        private readonly GlobalRepository<PagoSuscripcion> _pagoRepo;

        public DashboardAdminGlobalService()
        {
            _tenantRepo = new GlobalRepository<Tenant>();
            _pagoRepo = new GlobalRepository<PagoSuscripcion>();
        }

        public AdminGlobalDashboardViewModel GetDashboard(DateTime fecha)
        {
            var tenants = _tenantRepo.GetAll().ToList();

            // C2: fuente unica de verdad del estado = PlanesSuscripcion.CalcularEstado
            // (la misma que usa TenantService/Tenants panel). Elimina la logica DIY con
            // magic numbers y los strings "En Gracia"/"Por vencer" hardcodeados.
            string EstadoDe(Tenant t) =>
                PlanesSuscripcion.CalcularEstado(t.SuspendidoManual, t.PlanCodigo, t.FechaVencimiento, fecha);

            // Agrupaciones del dashboard global (operativos vs no operativos):
            //   Activos      = plan vigente (ACTIVO + POR VENCER)
            //   En Gracia    = vencido dentro de la gracia
            //   Suspendidos  = suspension manual o fuera de gracia (vencido)
            var activos = tenants.Count(t =>
                EstadoDe(t) == PlanesSuscripcion.EstadoActivo ||
                EstadoDe(t) == PlanesSuscripcion.EstadoPorVencer);

            var enGracia = tenants.Count(t => EstadoDe(t) == PlanesSuscripcion.EstadoEnGracia);

            var suspendidos = tenants.Count(t =>
                EstadoDe(t) == PlanesSuscripcion.EstadoSuspendidoManual ||
                EstadoDe(t) == PlanesSuscripcion.EstadoVencido);

            // C2 (bug): rango de dia calendario en vez de igualdad exacta de fecha.
            var finDelDia = fecha.Date.AddDays(1);
            var totalPagosHoy = _pagoRepo.GetAll()
                .Count(p => p.Fecha >= fecha.Date && p.Fecha < finDelDia);

            var tenantsById = tenants.ToDictionary(t => t.Id);

            var ultimosPagos = _pagoRepo.GetAll()
                .OrderByDescending(p => p.Fecha)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToList()
                .Select(p => new PagoSuscripcionDTO
                {
                    Tenant = tenantsById.TryGetValue(p.TenantId, out var t) ? t.Nombre : "Tenant #" + p.TenantId,
                    Monto = p.Monto,
                    Fecha = p.Fecha
                })
                .ToList();

            var viewModel = new AdminGlobalDashboardViewModel
            {
                TotalTenants = tenants.Count,
                TotalActivos = activos,
                TotalEnGracia = enGracia,
                TotalSuspendidos = suspendidos,
                TotalPagosHoy = totalPagosHoy,

                TenantsEnRiesgo = tenants
                    .Where(t =>
                    {
                        var est = EstadoDe(t);
                        return est == PlanesSuscripcion.EstadoEnGracia ||
                               est == PlanesSuscripcion.EstadoPorVencer;
                    })
                    .Select(t => new TenantAlertDTO
                    {
                        Id = t.Id,
                        Nombre = t.Nombre,
                        Estado = EstadoDe(t) == PlanesSuscripcion.EstadoEnGracia
                            ? "En Gracia"
                            : "Por vencer"
                    })
                    .ToList(),

                UltimosPagos = ultimosPagos
            };

            return viewModel;
        }
    }
}
