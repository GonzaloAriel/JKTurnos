using System;
using System.Security.Claims;
using System.Web;

namespace JKTurnos.Infrastructure
{
    public class TenantProvider : ITenantProvider
    {
        public int GetTenantId()
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;

            if (identity == null)
                throw new Exception("Usuario no autenticado");

            var claim = identity.FindFirst("TenantId");

            if (claim == null)
                throw new Exception("TenantId no encontrado");

            return int.Parse(claim.Value);
        }
    }
}