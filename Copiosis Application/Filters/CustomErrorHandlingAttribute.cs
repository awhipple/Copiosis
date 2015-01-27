using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Copiosis_Application.Controllers;

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
                AccountController controllerContext = (AccountController)filterContext.Controller;
                TempDataDictionary contextControllerTempData = filterContext.Controller.TempData;
                string subject = controllerContext.ACCOUNTERROR.ErrorSubject;
                string message = filterContext.Exception.Message;
                controllerContext.ACCOUNTERROR.ErrorMessage = message;
                if (subject == null || subject.Equals("")) //if no error subject was provided
                {
                    //set to default errsubject:
                    subject = "An internal error occured";
                }
                contextControllerTempData.Add(controllerContext.errorDictionaryKeys.ElementAt(0), subject);
                contextControllerTempData.Add(controllerContext.errorDictionaryKeys.ElementAt(1), message);
                filterContext.Result = new ViewResult
                {
                    ViewName = "Error",
                    TempData = contextControllerTempData
                };
            }
            filterContext.ExceptionHandled = true;
        }
    }
}
