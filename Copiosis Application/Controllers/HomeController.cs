using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

/* Home controller is used for pages viewed by a user that is not logged in */
namespace Copiosis_Application.Controllers
{
    public class HomeController : Controller
    {
        /* The landing page will be a login area, could redirect to the Login action? */
        public ActionResult Index()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                return RedirectToAction("Overview", "Account");
            }
        }
    }
}
