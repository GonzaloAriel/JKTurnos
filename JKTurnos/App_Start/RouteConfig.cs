using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace JKTurnos
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // D-042 / Posta 3: vidriera publica por slug (M-13). Antes de la ruta
            // default para que {controller} no la capture. Unica entrada publica.
            routes.MapRoute(
                name: "Vidriera",
                url: "vidriera/{slug}",
                defaults: new { controller = "Vidriera", action = "Index", slug = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
