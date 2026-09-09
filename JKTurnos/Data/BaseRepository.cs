using JKTurnos.Data;
using JKTurnos.Infrastructure;
using JKTurnos.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

public class BaseRepository<TEntity>
    where TEntity : BaseEntity, ITenantEntity
{
    protected readonly JKContext _context;

    public BaseRepository()
    {
        _context = new JKContext();
    }

    private int TenantId
    {
        get
        {
            return new TenantProvider().GetTenantId();
        }
    }

    public IQueryable<TEntity> GetAll()
    {
        return _context.Set<TEntity>()
            .Where(x => x.IsActive)
            .Where(x => x.TenantId == TenantId);
    }

    public TEntity GetById(int id)
    {
        return GetAll()
            .FirstOrDefault(x => x.Id == id);
    }

    // Incluye registros dados de baja (IsActive = false) dentro del tenant.
    // Solo lo usa el Admin del tenant para ver los eliminados/anulados.
    public IQueryable<TEntity> GetAllIncluyendoInactivos()
    {
        return _context.Set<TEntity>()
            .Where(x => x.TenantId == TenantId);
    }

    public void Add(TEntity entity)
    {
        entity.TenantId = TenantId;
        entity.CreatedAt = DateTime.Now;
        entity.IsActive = true;

        _context.Set<TEntity>().Add(entity);
        _context.SaveChanges();
    }

    public void Update(TEntity entity)
    {
        entity.UpdatedAt = DateTime.Now;

        var existing = _context.Set<TEntity>().Local.FirstOrDefault(e => e.Id == entity.Id);

        if (existing != null && !ReferenceEquals(existing, entity))
        {
            var createdAt = existing.CreatedAt;
            var isActive = existing.IsActive;
            var tenantId = existing.TenantId;

            _context.Entry(existing).CurrentValues.SetValues(entity);

            existing.CreatedAt = createdAt;
            existing.IsActive = isActive;
            existing.TenantId = tenantId;
            existing.UpdatedAt = DateTime.Now;

            _context.Entry(existing).State = EntityState.Modified;
        }
        else
        {
            // M-10: fail-closed: recargar la entidad persistida para validar tenant
            // y preservar TenantId/IsActive/CreatedAt (evita attach de entidad foranea
            // con TenantId alterable).
            var persistida = _context.Set<TEntity>()
                .FirstOrDefault(e => e.Id == entity.Id);

            if (persistida == null)
                throw new System.InvalidOperationException("Registro no encontrado.");

            if (persistida.TenantId != TenantId)
                throw new System.UnauthorizedAccessException("No tiene permisos para modificar este registro.");

            var createdAt = persistida.CreatedAt;
            var tenantId = persistida.TenantId;

            _context.Entry(persistida).CurrentValues.SetValues(entity);

            persistida.CreatedAt = createdAt;
            persistida.TenantId = tenantId;
            persistida.UpdatedAt = DateTime.Now;

            _context.Entry(persistida).State = EntityState.Modified;
        }

        _context.SaveChanges();
    }

    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        var list = entities as IList<TEntity> ?? entities.ToList();

        foreach (var entity in list)
        {
            entity.UpdatedAt = DateTime.Now;

            var existing = _context.Set<TEntity>().Local.FirstOrDefault(e => e.Id == entity.Id);

            if (existing != null && !ReferenceEquals(existing, entity))
            {
                var createdAt = existing.CreatedAt;
                var isActive = existing.IsActive;
                var tenantId = existing.TenantId;

                _context.Entry(existing).CurrentValues.SetValues(entity);

                existing.CreatedAt = createdAt;
                existing.IsActive = isActive;
                existing.TenantId = tenantId;
                existing.UpdatedAt = DateTime.Now;

                _context.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                // M-10: mismo guard fail-closed que Update.
                var persistida = _context.Set<TEntity>()
                    .FirstOrDefault(e => e.Id == entity.Id);

                if (persistida == null)
                    throw new System.InvalidOperationException("Registro no encontrado.");

                if (persistida.TenantId != TenantId)
                    throw new System.UnauthorizedAccessException("No tiene permisos para modificar este registro.");

                var createdAt = persistida.CreatedAt;
                var tenantId = persistida.TenantId;

                _context.Entry(persistida).CurrentValues.SetValues(entity);

                persistida.CreatedAt = createdAt;
                persistida.TenantId = tenantId;
                persistida.UpdatedAt = DateTime.Now;

                _context.Entry(persistida).State = EntityState.Modified;
            }
        }

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

    public void Restore(int id)
    {
        var entity = GetAllIncluyendoInactivos()
            .FirstOrDefault(x => x.Id == id);

        if (entity == null)
            return;

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
    }
}