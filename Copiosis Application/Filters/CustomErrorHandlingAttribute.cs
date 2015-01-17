using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Copiosis_Application.Filters
{
    public class CustomErrorHandlingAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled == true)
            {
                return;
            }
            else
            {
                string actionName = filterContext.RouteData.Values["action"].ToString();
                if (actionName.Equals("AddItem")) 
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "Error",
                        TempData = filterContext.Controller.TempData
                    };
                }
            }
            filterContext.ExceptionHandled = true;
        }
    }
}
