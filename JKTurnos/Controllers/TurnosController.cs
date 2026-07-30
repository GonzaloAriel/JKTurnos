using System;
using System.Web.Mvc;

namespace JKTurnos.Controllers
{
    [Authorize]
    public class TurnosController : BaseController
    {
        public ActionResult Agenda()
        {
            var today = DateTime.Now;

            var dayNames = new[] { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };
            var monthNames = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            ViewBag.CurrentDateDisplay = dayNames[(int)today.DayOfWeek] + " " + today.Day + " " + monthNames[today.Month - 1] + ", " + today.Year;

            return View();
        }
    }
}
