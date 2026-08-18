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

        public List<Recurso> GetFiltrados(string busqueda = null, TipoRecurso? tipo = null)
        {
            var query = _repo.GetAll();

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
        }

        public void Update(Recurso recurso)
        {
            _repo.Update(recurso);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}
