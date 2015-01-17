using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Copiosis_Application.Models
{
    public class ErrorModel
    {
        public string TempDataKey { get; set; }

        public string TempDataVal { get; set; }

        public string ErrorSubject { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorDetails { get; set; }

        public int HttpErrorCode { get; set; }
    }
}