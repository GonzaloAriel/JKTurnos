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

        public Servicio GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Create(Servicio servicio)
        {
            _repo.Add(servicio);
        }

        public void Update(Servicio servicio)
        {
            _repo.Update(servicio);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}