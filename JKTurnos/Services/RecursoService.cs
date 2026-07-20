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
