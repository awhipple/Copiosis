using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Copiosis_Application.Filters;
using Copiosis_Application.Models;
using Copiosis_Application.DB_Data;

/* Home controller is used for pages viewed by a user that is not logged in */
namespace Copiosis_Application.Controllers
{
    [CustomErrorHandling]
    public class HomeController : Controller
    {
        private Models.ErrorModel HOMEERROR = new Models.ErrorModel();
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

        #region Helpers
        public Models.ErrorModel getError()
        {
            return this.HOMEERROR;
        }
        #endregion
    }


}
