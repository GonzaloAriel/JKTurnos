using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    // Auditoria de acciones (quien-hizo-que-cuando), visible solo para Admin.
    // Listado con filtros (fecha, usuario, entidad, accion) y paginacion.
    [Authorize(Roles = Roles.Admin)]
    public class AuditoriaController : BaseController
    {
        private const int PageSize = 25;

        private static readonly string[] Entidades = {
            AuditService.E_Turno,
            AuditService.E_Cliente,
            AuditService.E_Servicio,
            AuditService.E_Recurso,
            AuditService.E_Usuario,
            AuditService.E_Pago,
            AuditService.E_EstadoTurno,
            AuditService.E_Configuracion,
            AuditService.E_Autenticacion
        };

        private static readonly string[] Acciones = {
            AuditService.A_Crear,
            AuditService.A_Editar,
            AuditService.A_Eliminar,
            AuditService.A_CambiarEstado,
            AuditService.A_Pago,
            AuditService.A_Anular,
            AuditService.A_Login,
            AuditService.A_LoginFallido,
            AuditService.A_Logout,
            AuditService.A_ResetPassword
        };

        public ActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta, int? usuarioId,
            string entidad, string accion, int? pagina)
        {
            var repo = new BaseRepository<Auditoria>();
            var query = repo.GetAll();

            if (fechaDesde.HasValue)
            {
                var desde = fechaDesde.Value.Date;
                query = query.Where(a => a.Fecha >= desde);
            }

            if (fechaHasta.HasValue)
            {
                var hasta = fechaHasta.Value.Date.AddDays(1);
                query = query.Where(a => a.Fecha < hasta);
            }

            if (usuarioId.HasValue)
                query = query.Where(a => a.UsuarioId == usuarioId.Value);

            if (!string.IsNullOrWhiteSpace(entidad))
            {
                var e = entidad.Trim();
                query = query.Where(a => a.Entidad == e);
            }

            if (!string.IsNullOrWhiteSpace(accion))
            {
                var ac = accion.Trim();
                query = query.Where(a => a.Accion == ac);
            }

            var total = query.Count();

            var totalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            if (totalPaginas == 0)
                totalPaginas = 1;

            var page = pagina ?? 1;
            if (page < 1)
                page = 1;
            if (page > totalPaginas)
                page = totalPaginas;

            var items = query
                .OrderByDescending(a => a.Fecha)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.Usuarios = new UserService().GetAll()
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Nombre ?? u.Email
                });

            ViewBag.Entidades = Entidades
                .Select(e => new SelectListItem { Value = e, Text = e });

            ViewBag.Acciones = Acciones
                .Select(a => new SelectListItem { Value = a, Text = a });

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.UsuarioId = usuarioId;
            ViewBag.Entidad = entidad;
            ViewBag.Accion = accion;
            ViewBag.Pagina = page;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.Total = total;

            return View(items);
        }
    }
}