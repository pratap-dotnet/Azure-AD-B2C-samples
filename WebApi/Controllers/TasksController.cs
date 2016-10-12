using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Authorize]
    public class TasksController : ApiController
    {
        // GET api/values
        public IEnumerable<UserTask> Get()
        {
            return new UserTask[] { new UserTask { Id= 0, CompletedDate = DateTime.Today, Description = "Task 1", Name ="Task 1", StartDate = DateTime.Today.AddDays(-1)},
            new UserTask { Id= 1,CompletedDate = DateTime.Today, Description = "Task 2", Name ="Task2", StartDate = DateTime.Today.AddDays(-1)}};
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
