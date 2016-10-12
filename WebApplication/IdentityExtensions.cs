using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace WebApplication
{
    public static class IdentityExtensions
    {
        public static string GetTenantId(this ClaimsIdentity identity)
        {
            return identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
        }

        public static string GetName(this ClaimsIdentity identity)
        {
            return identity.FindFirst("name").Value;
        }

        public static string GetName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("name").Value;
        }

        public static string GetIssuer(this ClaimsIdentity identity)
        {
            return identity.FindFirst("iss").Value;
        }

        public static string GetUniqueUserName(this ClaimsIdentity identity)
        {
            var splitName = identity.GetName().Split('#');
            return splitName[splitName.Length - 1];
        }

        public static string GetUniqueUserName(this ClaimsPrincipal principal)
        {
            var splitName = principal.GetName().Split('#');
            return splitName[splitName.Length - 1];
        }
    }
}