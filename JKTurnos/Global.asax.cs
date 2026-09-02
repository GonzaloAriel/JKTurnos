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

        // M-08: el subdominio publico raiz (http://tenant.dominio/) llega al portal
        // de reservas en lugar del Home privado [Authorize]. SOLO se reescribe el
        // path raiz "/" y SOLO si el host resuelve a un tenant con portal habilitado;
        // cualquier otra URL privada queda intacta (no se toca la autorizacion).
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var ctx = HttpContext.Current;

            if (ctx == null || ctx.Request == null)
                return;

            var path = ctx.Request.AppRelativeCurrentExecutionFilePath ?? string.Empty;

            if (path != "~/" && path != "~\\")
                return;

            var host = (!string.IsNullOrWhiteSpace(ctx.Request.Headers["X-Forwarded-Host"])
                ? ctx.Request.Headers["X-Forwarded-Host"]
                : ctx.Request.Url.Host);

            // MODO DEV (reversible): en la maquina local (p.ej. localhost) la raiz "/"
            // vuelve al flujo normal del panel (login -> command center), NO al portal.
            // El portal queda accesible en /Portal/Inicio. Se detecta el host del entorno
            // de desarrollo por appSetting JKPortalHostDevSinSubdominioParaAdmin.
            // En produccion (sin ese appSetting) la raiz "*.<dominio>/" sigue al portal.
            var matrizDev = (System.Configuration.ConfigurationManager
                .AppSettings["JKPortalHostDevSinSubdominioParaAdmin"] ?? "").Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(matrizDev)
                && matrizDev.Split(',').Contains(host.ToLower()))
            {
                return; // no reescribir: la raiz es el panel (login -> dashboard)
            }

            try
            {
                var service = new Services.PortalReservaService();
                var tenant = service.ResolveTenantByHost(host);

                if (tenant != null && service.PortalDisponible(tenant))
                {
                    ctx.RewritePath("/Portal/Inicio", false);
                }
            }
            catch
            {
                // Cualquier fallo de resolucion del portal: se sigue al path original.
            }
        }
    }
}
