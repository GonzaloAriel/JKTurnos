using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    public class ClienteService
    {
        private readonly BaseRepository<Cliente> _repo;

        public ClienteService()
        {
            _repo = new BaseRepository<Cliente>();
        }

        public List<Cliente> GetAll()
        {
            return _repo.GetAll()
                        .OrderBy(x => x.Nombre)
                        .ToList();
        }

        public Cliente GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Create(Cliente cliente)
        {
            _repo.Add(cliente);
        }

        public void Update(Cliente cliente)
        {
            _repo.Update(cliente);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }
    }
}