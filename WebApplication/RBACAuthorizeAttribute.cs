using System;
using System.Configuration;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

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

        
        private static string adminGroupId = ConfigurationManager.AppSettings["adminGroup"];
        private static string nurseGroupId = ConfigurationManager.AppSettings["nurseGroup"];
        private static string doctorGroupId = ConfigurationManager.AppSettings["doctorGroup"];
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
                            if (group.Value.Contains(GetGroupId(item)))
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

        private string GetGroupId(string item)
        {
            var groupId = string.Empty;
            switch (item)
            {
                case Groups.Admin:
                    groupId = adminGroupId;
                    break;
                case Groups.Doctor:
                    groupId = doctorGroupId;
                    break;
                case Groups.Nurse:
                    groupId = nurseGroupId;
                    break;
                default:
                    break;
            }
            return groupId;
        }
    }
}