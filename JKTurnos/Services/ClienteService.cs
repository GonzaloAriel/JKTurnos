using System;
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

        public List<Cliente> GetFiltrados(string busqueda = null, bool incluirEliminados = false)
        {
            var query = incluirEliminados
                ? _repo.GetAllIncluyendoInactivos()
                : _repo.GetAll();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var termino = busqueda.Trim();

                query = query.Where(c =>
                    (c.Nombre != null && c.Nombre.Contains(termino)) ||
                    (c.Telefono != null && c.Telefono.Contains(termino)) ||
                    (c.Email != null && c.Email.Contains(termino)));
            }

            return query.OrderBy(c => c.Nombre)
                        .ToList();
        }

        public Cliente GetById(int id)
        {
            return _repo.GetById(id);
        }

        public void Create(Cliente cliente)
        {
            _repo.Add(cliente);

            new AuditService().Registrar(
                AuditService.A_Crear,
                AuditService.E_Cliente,
                "Creo el cliente " + cliente.Nombre,
                cliente.Id);
        }

        public void Update(Cliente cliente)
        {
            var existente = _repo.GetById(cliente.Id);

            if (existente == null)
                throw new ArgumentException("Cliente no encontrado.");

            var cambios = CompararClientes(existente, cliente);

            _repo.Update(cliente);

            new AuditService().Registrar(
                AuditService.A_Editar,
                AuditService.E_Cliente,
                "Edito el cliente " + cliente.Nombre,
                cliente.Id,
                cambios);
        }

        public void Delete(int id)
        {
            var cliente = _repo.GetById(id);

            _repo.Delete(id);

            new AuditService().Registrar(
                AuditService.A_Eliminar,
                AuditService.E_Cliente,
                cliente != null ? "Elimino el cliente " + cliente.Nombre : "Elimino el cliente #" + id,
                id);
        }

        // Restaura un cliente dado de baja (IsActive = true). Solo Admin.
        public void Restaurar(int id)
        {
            var cliente = _repo.GetAllIncluyendoInactivos()
                .FirstOrDefault(x => x.Id == id);

            if (cliente == null)
                throw new ArgumentException("Cliente no encontrado.");

            _repo.Restore(id);

            new AuditService().Registrar(
                AuditService.A_Restaurar,
                AuditService.E_Cliente,
                "Restauro el cliente " + cliente.Nombre,
                id);
        }

        private static string CompararClientes(Cliente a, Cliente b)
        {
            return AuditService.FormatearCambios(
                AuditService.Campo("Nombre", a.Nombre, b.Nombre),
                AuditService.Campo("Telefono", a.Telefono, b.Telefono),
                AuditService.Campo("Email", a.Email, b.Email),
                AuditService.Campo("Direccion", a.Direccion, b.Direccion),
                AuditService.Campo("Notas", a.Notas, b.Notas));
        }
    }
}