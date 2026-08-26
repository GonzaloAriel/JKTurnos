using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Services
{
    public class DashboardAdminGlobalService
    {
        private readonly JKContext _db;

        public DashboardAdminGlobalService()
        {
            _db = new JKContext();
        }

        public AdminGlobalDashboardViewModel GetDashboard(DateTime fecha)
        {
            var tenants = _db.Tenants.Where(t => t.IsActive).ToList();

            var viewModel = new AdminGlobalDashboardViewModel
            {
                TotalTenants = tenants.Count,
                TotalActivos = tenants.Count(t => !t.SuspendidoManual && (t.FechaVencimiento ?? DateTime.MinValue) >= fecha),
                TotalEnGracia = tenants.Count(t => !t.SuspendidoManual && (t.FechaVencimiento ?? DateTime.MinValue) < fecha && (t.FechaVencimiento ?? DateTime.MinValue).AddDays(5) >= fecha),
                TotalSuspendidos = tenants.Count(t => t.SuspendidoManual || (t.FechaVencimiento ?? DateTime.MinValue).AddDays(5) < fecha),
                TotalPagosHoy = _db.PagosSuscripciones.Count(p => p.Fecha == fecha),
                
                TenantsEnRiesgo = tenants
                    .Where(t => !t.SuspendidoManual && (t.FechaVencimiento ?? DateTime.MinValue).AddDays(5) >= fecha && (t.FechaVencimiento ?? DateTime.MinValue) < fecha.AddDays(3))
                    .Select(t => new TenantAlertDTO { 
                        Id = t.Id, 
                        Nombre = t.Nombre, 
                        Estado = (t.FechaVencimiento ?? DateTime.MinValue) < fecha ? "En Gracia" : "Por vencer" 
                    })
                    .ToList(),

                UltimosPagos = _db.PagosSuscripciones
                    .OrderByDescending(p => p.Fecha)
                    .Take(5)
                    .Select(p => new PagoSuscripcionDTO { 
                        Tenant = _db.Tenants.FirstOrDefault(t => t.Id == p.TenantId).Nombre, 
                        Monto = p.Monto, 
                        Fecha = p.Fecha 
                    })
                    .ToList()
            };

            return viewModel;
        }
    }
}
