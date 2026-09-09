using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    public class RecursoService
    {
        private readonly BaseRepository<Recurso> _repo;

        public RecursoService()
        {
            _repo = new BaseRepository<Recurso>();
        }

        public List<Recurso> GetAll()
        {
            return _repo.GetAll()
                        .OrderBy(x => x.Nombre)
                        .ToList();
        }

        public List<Recurso> GetFiltrados(string busqueda = null, TipoRecurso? tipo = null, bool incluirEliminados = false)
        {
            var query = incluirEliminados
                ? _repo.GetAllIncluyendoInactivos()
                : _repo.GetAll();

            if (tipo.HasValue)
                query = query.Where(r => r.Tipo == tipo.Value);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim();

                query = query.Where(r =>
                    (r.Nombre != null && r.Nombre.Contains(termino)) ||
                    (r.Descripcion != null && r.Descripcion.Contains(termino)) ||
                    (r.Color != null && r.Color.Contains(termino)));
            }

            return query.OrderBy(r => r.Nombre)
                        .ToList();
        }

        public Recurso GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Create(Recurso recurso)
        {
            _repo.Add(recurso);

            new AuditService().Registrar(
                AuditService.A_Crear,
                AuditService.E_Recurso,
                "Creo el recurso " + recurso.Nombre,
                recurso.Id);
        }

        public void Update(Recurso recurso)
        {
            var existente = _repo.GetById(recurso.Id);

            if (existente == null)
                throw new ArgumentException("Recurso no encontrado.");

            var cambios = CompararRecursos(existente, recurso);

            _repo.Update(recurso);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Recurso,
                "Edito el recurso " + recurso.Nombre,
                recurso.Id,
                cambios);
        }

        public void Delete(int id)
        {
            var recurso = _repo.GetById(id);

            _repo.Delete(id);

            new AuditService().Registrar(
                AuditService.A_Eliminar,
                AuditService.E_Recurso,
                recurso != null ? "Elimino el recurso " + recurso.Nombre : "Elimino el recurso #" + id,
                id);
        }

        // Restaura un recurso dado de baja (IsActive = true). Solo Admin.
        public void Restaurar(int id)
        {
            var recurso = _repo.GetAllIncluyendoInactivos()
                .FirstOrDefault(x => x.Id == id);

            if (recurso == null)
                throw new ArgumentException("Recurso no encontrado.");

            _repo.Restore(id);

            new AuditService().Registrar(
                AuditService.A_Restaurar,
                AuditService.E_Recurso,
                "Restauro el recurso " + recurso.Nombre,
                id);
        }

        private static string CompararRecursos(Recurso a, Recurso b)
        {
            return AuditService.FormatearCambios(
                AuditService.Campo("Nombre", a.Nombre, b.Nombre),
                AuditService.Campo("Descripcion", a.Descripcion, b.Descripcion),
                AuditService.Campo("Tipo", a.Tipo.ToString(), b.Tipo.ToString()),
                AuditService.Campo("Color", a.Color, b.Color));
        }
    }
}
