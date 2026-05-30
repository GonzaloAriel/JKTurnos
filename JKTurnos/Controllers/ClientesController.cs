using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize]
    public class ClientesController : BaseController
    {
        private readonly ClienteService _service;

        public ClientesController()
        {
            _service = new ClienteService();
        }

        // GET: Clientes
        public ActionResult Index()
        {
            var clientes = _service.GetAll();
            return View(clientes);
        }

        // GET: Clientes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cliente model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _service.Create(model);

            return RedirectToAction("Index");
        }

        // GET: Clientes/Edit/5
        public ActionResult Edit(int id)
        {
            var cliente = _service.GetById(id);

            if (cliente == null)
                return HttpNotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Cliente model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _service.Update(model);

            return RedirectToAction("Index");
        }

        // GET: Clientes/Delete/5
        public ActionResult Delete(int id)
        {
            var cliente = _service.GetById(id);

            if (cliente == null)
                return HttpNotFound();

            return View(cliente);
        }

        // POST: Clientes/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _service.Delete(id);

            return RedirectToAction("Index");
        }
    }
}