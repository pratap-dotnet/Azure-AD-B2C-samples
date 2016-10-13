using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;

namespace SecondWebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
        {
        }

        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" }, Startup.SignInPolicyId);
            }
        }
    }
}