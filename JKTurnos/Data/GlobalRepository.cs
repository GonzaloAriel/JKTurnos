using System;
using System.Data.Entity;
using System.Linq;
using JKTurnos.Models.Entities;

namespace JKTurnos.Data
{
    /// <summary>
    /// D-022: Repositorio de plataforma. Filtra SOLO por IsActive, sin TenantId.
    /// Uso exclusivo de servicios de plataforma (TenantService y la via global de
    /// UserService). NOTA: Add NO asigna TenantId; el llamador lo setea explicitamente.
    /// Efecto colateral deseado: un AdminGlobal jamas ve datos de negocio a traves de
    /// BaseRepository (le devuelve vacio), blindando los modulos operativos.
    /// </summary>
    public class GlobalRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected readonly JKContext _context;

        public GlobalRepository()
        {
            _context = new JKContext();
        }

        public IQueryable<TEntity> GetAll()
        {
            return _context.Set<TEntity>()
                .Where(x => x.IsActive);
        }

        public TEntity GetById(int id)
        {
            return GetAll()
                .FirstOrDefault(x => x.Id == id);
        }

        public void Add(TEntity entity)
        {
            entity.CreatedAt = DateTime.Now;
            entity.IsActive = true;

            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = GetById(id);

            if (entity == null)
                return;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
        }
    }
}