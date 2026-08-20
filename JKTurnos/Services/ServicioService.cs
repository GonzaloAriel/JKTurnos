using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    public class ServicioService
    {
        private readonly BaseRepository<Servicio> _repo;

        public ServicioService()
        {
            _repo = new BaseRepository<Servicio>();
        }

        public List<Servicio> GetAll()
        {
            return _repo.GetAll()
                        .OrderBy(x => x.Nombre)
                        .ToList();
        }

        public List<Servicio> GetFiltrados(string busqueda = null)
        {
            var query = _repo.GetAll();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim();

                query = query.Where(s =>
                    (s.Nombre != null && s.Nombre.Contains(termino)) ||
                    (s.Descripcion != null && s.Descripcion.Contains(termino)));
            }

            return query.OrderBy(s => s.Nombre)
                        .ToList();
        }

        public Servicio GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Create(Servicio servicio)
        {
            _repo.Add(servicio);

            new AuditService().Registrar(
                AuditService.A_Crear,
                AuditService.E_Servicio,
                "Creo el servicio " + servicio.Nombre,
                servicio.Id);
        }

        public void Update(Servicio servicio)
        {
            var existente = _repo.GetById(servicio.Id);

            if (existente == null)
                throw new ArgumentException("Servicio no encontrado.");

            var cambios = CompararServicios(existente, servicio);

            _repo.Update(servicio);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Servicio,
                "Edito el servicio " + servicio.Nombre,
                servicio.Id,
                cambios);
        }

        public void Delete(int id)
        {
            var servicio = _repo.GetById(id);

            _repo.Delete(id);

            new AuditService().Registrar(
                AuditService.A_Eliminar,
                AuditService.E_Servicio,
                servicio != null ? "Elimino el servicio " + servicio.Nombre : "Elimino el servicio #" + id,
                id);
        }

        private static string CompararServicios(Servicio a, Servicio b)
        {
            return AuditService.FormatearCambios(
                AuditService.Campo("Nombre", a.Nombre, b.Nombre),
                AuditService.Campo("Descripcion", a.Descripcion, b.Descripcion),
                AuditService.Campo("Precio", a.Precio.ToString("0.##"), b.Precio.ToString("0.##")),
                AuditService.Campo("DuracionMinutos", a.DuracionMinutos.ToString(), b.DuracionMinutos.ToString()));
        }
    }
}