using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Data;
using JKTurnos.Models.Entities;

namespace JKTurnos.Services
{
    public class RecursoServicioService
    {
        private readonly BaseRepository<RecursoServicio> _repo;

        public RecursoServicioService()
        {
            _repo = new BaseRepository<RecursoServicio>();
        }

        public List<int> GetServicioIdsPorRecurso(int recursoId)
        {
            return _repo.GetAll()
                        .Where(rs => rs.RecursoId == recursoId)
                        .Select(rs => rs.ServicioId)
                        .ToList();
        }

        public List<int> GetRecursoIdsPorServicio(int servicioId)
        {
            return _repo.GetAll()
                        .Where(rs => rs.ServicioId == servicioId)
                        .Select(rs => rs.RecursoId)
                        .ToList();
        }

        public void SetRecursosParaServicio(int servicioId, List<int> recursoIds, int tenantId)
        {
            using (var db = new JKContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var existingRelations = db.RecursoServicios
                            .Where(rs => rs.ServicioId == servicioId && rs.TenantId == tenantId)
                            .ToList();

                        foreach (var recursoId in recursoIds)
                        {
                            var relation = existingRelations.FirstOrDefault(rs => rs.RecursoId == recursoId);
                            if (relation != null)
                            {
                                if (!relation.IsActive)
                                {
                                    relation.IsActive = true;
                                    relation.UpdatedAt = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.RecursoServicios.Add(new RecursoServicio
                                {
                                    RecursoId = recursoId,
                                    ServicioId = servicioId,
                                    TenantId = tenantId,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now
                                });
                            }
                        }

                        var toDeactivate = existingRelations
                            .Where(rs => rs.IsActive && !recursoIds.Contains(rs.RecursoId));

                        foreach (var relation in toDeactivate)
                        {
                            relation.IsActive = false;
                            relation.UpdatedAt = DateTime.Now;
                        }

                        db.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void SetServiciosParaRecurso(int recursoId, List<int> servicioIds, int tenantId)
        {
            using (var db = new JKContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var existingRelations = db.RecursoServicios
                            .Where(rs => rs.RecursoId == recursoId && rs.TenantId == tenantId)
                            .ToList();

                        foreach (var servicioId in servicioIds)
                        {
                            var relation = existingRelations.FirstOrDefault(rs => rs.ServicioId == servicioId);
                            if (relation != null)
                            {
                                if (!relation.IsActive)
                                {
                                    relation.IsActive = true;
                                    relation.UpdatedAt = DateTime.Now;
                                }
                            }
                            else
                            {
                                db.RecursoServicios.Add(new RecursoServicio
                                {
                                    RecursoId = recursoId,
                                    ServicioId = servicioId,
                                    TenantId = tenantId,
                                    IsActive = true,
                                    CreatedAt = DateTime.Now
                                });
                            }
                        }

                        var toDeactivate = existingRelations
                            .Where(rs => rs.IsActive && !servicioIds.Contains(rs.ServicioId));
                            
                        foreach (var relation in toDeactivate)
                        {
                            relation.IsActive = false;
                            relation.UpdatedAt = DateTime.Now;
                        }

                        db.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
