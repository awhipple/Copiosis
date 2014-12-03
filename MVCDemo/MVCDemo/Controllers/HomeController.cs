using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCDemo.Something;
using MVCDemo.Models;

namespace MVCDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var db = new SpyModel();
            List<AgentModel> agents = db.Agents.Where(a => a.salary > 300000).Select(g => new AgentModel{ FirstName = g.first, LastName = g.last, Salary = (int)g.salary }).ToList();
            
            return View(agents);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
