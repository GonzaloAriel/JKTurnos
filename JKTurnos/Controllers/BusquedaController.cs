using System.Linq;
using System.Web.Mvc;
using JKTurnos.Services;
using JKTurnos.Models.ViewModels;

namespace JKTurnos.Controllers
{
    // M-02: Busqueda global del topbar. Endpoint GET de autocomplete tipificado.
    [Authorize]
    public class BusquedaController : BaseController
    {
        private readonly BusquedaService _service;

        public BusquedaController()
        {
            _service = new BusquedaService();
        }

        public JsonResult Buscar(string termino = null)
        {
            var items = _service.Buscar(termino, 10)
                .Select(item =>
                {
                    string url;
                    switch (item.Tipo)
                    {
                        case "cliente":
                            url = Url.Action("Details", "Clientes", new { id = item.Id });
                            break;
                        case "servicio":
                            url = Url.Action("Edit", "Servicios", new { id = item.Id });
                            break;
                        case "recurso":
                            url = Url.Action("Edit", "Recursos", new { id = item.Id });
                            break;
                        case "turno":
                            url = Url.Action("Agenda", "Turnos", new { openTurnoId = item.Id });
                            break;
                        default:
                            url = null;
                            break;
                    }

                    return new
                    {
                        tipo = item.Tipo,
                        id = item.Id,
                        label = item.Label,
                        sub = item.Sub,
                        url
                    };
                })
                .Where(x => x.url != null)
                .ToList();

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(string termino = null)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return View(new BusquedaViewModel());

            var items = _service.Buscar(termino, 50);
            var model = new BusquedaViewModel
            {
                Termino = termino,
                Clientes = items.Where(x => x.Tipo == "cliente").ToList(),
                Servicios = items.Where(x => x.Tipo == "servicio").ToList(),
                Recursos = items.Where(x => x.Tipo == "recurso").ToList(),
                Turnos = items.Where(x => x.Tipo == "turno").ToList()
            };
            model.TotalResultados = model.Clientes.Count + model.Servicios.Count + model.Recursos.Count + model.Turnos.Count;
            return View(model);
        }
    }
}
