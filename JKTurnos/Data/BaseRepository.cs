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
            _context.Entry(entity).State = EntityState.Modified;
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
                _context.Entry(entity).State = EntityState.Modified;
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
}