using System;
using System.Web.Mvc;
using JKTurnos.Models.Entities;
using JKTurnos.Models.ViewModels;
using JKTurnos.Services;

namespace JKTurnos.Controllers
{
    [Authorize(Roles = Roles.AdminGlobal)]
    public class AvataresController : BaseController
    {
        private readonly AvatarPlataformaService _service;

        public AvataresController()
        {
            _service = new AvatarPlataformaService();
        }

        public ActionResult Index()
        {
            return View(_service.GetAll());
        }

        public ActionResult Crear()
        {
            return View(new AvatarPlataformaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(AvatarPlataformaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.Crear(model.Nombre, model.Url);
                TempData["Success"] = "Avatar creado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public ActionResult Editar(int id)
        {
            var avatar = _service.GetById(id);

            if (avatar == null)
                return HttpNotFound();

            var vm = new AvatarPlataformaViewModel
            {
                Id = avatar.Id,
                Nombre = avatar.Nombre,
                Url = avatar.Url
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(AvatarPlataformaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _service.Actualizar(model.Id, model.Nombre, model.Url);
                TempData["Success"] = "Avatar actualizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public ActionResult Eliminar(int id)
        {
            var avatar = _service.GetById(id);

            if (avatar == null)
                return HttpNotFound();

            return View(avatar);
        }

        [HttpPost]
        [ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarConfirmed(int id)
        {
            _service.Eliminar(id);
            TempData["Success"] = "Avatar eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}
