using System;
using System.Configuration;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication
{
    public static class Groups
    {
        public const string Nurse = "Nurse";
        public const string Doctor = "Doctor";
        public const string Admin = "Admin";
    }

    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Method, Inherited =true, AllowMultiple =true)]
    public class RBACAuthorizeAttribute : AuthorizeAttribute
    {

        private readonly string[] groupNames;
        public RBACAuthorizeAttribute(params string[] groupNames)
        {
            this.groupNames = groupNames;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var result = false;
            if(httpContext.User != null &&  (httpContext.User.Identity is ClaimsIdentity))
            {
                var claims = httpContext.User.Identity as ClaimsIdentity;
                if(groupNames != null && groupNames.Length > 0)
                {
                    var group = claims.FindFirst("groups");
                    if (group != null)
                    {
                        foreach (var item in groupNames)
                        {
                            if (group.Value.Contains(item))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }else
                {
                    result = true;
                }
                
            }
            return result;
        }

        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));
            }
        }
    }
}