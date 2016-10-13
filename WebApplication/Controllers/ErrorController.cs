using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{

    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(string message)
        {
            ViewBag.Message = message;
            return View("Error");
        }

        public ActionResult AccessDenied()
        {
            ViewBag.Message = "You are unauthorized to perform the action";
            return View("Error");
        }
    }
}