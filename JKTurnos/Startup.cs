using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Web.Helpers;
using System.Security.Claims;

[assembly: OwinStartup(typeof(JKTurnos.Startup))]

namespace JKTurnos
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier =
            ClaimTypes.NameIdentifier;

            app.UseCookieAuthentication(
                new CookieAuthenticationOptions
                {
                    AuthenticationType = "ApplicationCookie",
                    LoginPath = new PathString("/Account/Login")
                });
        }
    }
}