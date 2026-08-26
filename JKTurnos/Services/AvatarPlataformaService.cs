using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    public class AvatarPlataformaService
    {
        private readonly GlobalRepository<AvatarPlataforma> _repo;

        public AvatarPlataformaService()
        {
            _repo = new GlobalRepository<AvatarPlataforma>();
        }

        public List<AvatarPlataforma> GetAll()
        {
            return _repo.GetAll()
                        .OrderBy(x => x.Nombre)
                        .ToList();
        }

        public AvatarPlataforma GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Crear(string nombre, string url)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new System.ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(url))
                throw new System.ArgumentException("La URL es obligatoria.");

            var avatar = new AvatarPlataforma
            {
                Nombre = nombre.Trim(),
                Url = url.Trim()
            };

            _repo.Add(avatar);
        }

        public void Actualizar(int id, string nombre, string url)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new System.ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(url))
                throw new System.ArgumentException("La URL es obligatoria.");

            var actual = _repo.GetById(id);

            if (actual == null)
                throw new System.InvalidOperationException("Avatar no encontrado.");

            actual.Nombre = nombre.Trim();
            actual.Url = url.Trim();
            _repo.Update(actual);
        }

        public void Eliminar(int id)
        {
            _repo.Delete(id);
        }
    }
}
