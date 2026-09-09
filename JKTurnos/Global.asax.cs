using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using JKTurnos.Data;
using JKTurnos.Migrations;

namespace JKTurnos
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Al publicar, la app actualiza sola la base de datos: aplica las
            // migraciones pendientes y ejecuta el Seed (idempotente).
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<JKContext, Configuration>()
            );
        }

        // SECURITY: headers defensivos para todas las respuestas (M-3/M-4/M-5).
        protected void Application_PreSendRequestHeaders()
        {
            var response = Response;
            if (response == null || response.Headers == null)
                return;

            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["X-Frame-Options"] = "SAMEORIGIN";
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            response.AddHeader("X-XSS-Protection", "1; mode=block");

            // M-12: HSTS solo sobre HTTPS (Forzar HTTPS en hosting).
            var req = HttpContext.Current != null ? HttpContext.Current.Request : null;
            if (req != null && req.IsSecureConnection)
            {
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }
        }
    }
}
