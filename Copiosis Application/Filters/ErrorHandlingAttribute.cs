//This is setup to be 
using System;
using System.Web.Mvc;

namespace Copiosis_Application.Filters
{
    public class ErrorHandlingAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Shared/Error.cshtml");
            filterContext.ExceptionHandled = true;
        }
    }
}