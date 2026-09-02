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
    }
}
