using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApi.Models;

namespace WebApplication.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        // GET: Tasks
        public async Task<ActionResult> Index()
        {
            var bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as System.IdentityModel.Tokens.BootstrapContext;

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ConfigurationManager.AppSettings["webApi"] + "/api/tasks");

            // Add the token acquired from ADAL to the request headers
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bootstrapContext.Token);
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                String responseString = await response.Content.ReadAsStringAsync();

                ViewBag.Tasks = JsonConvert.DeserializeObject<UserTask>(responseString);
                return View();
            }
            return new EmptyResult();
        }
    }
}