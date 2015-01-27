using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;
using System.Web.Mvc;
using Copiosis_Application.DB_Data;

namespace Copiosis_Application.Models
{
    public class AddClassModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string name { get; set; }

        [Required(ErrorMessage = "SuggestedGateway is required")]
        public int suggestedGateway { get; set; }

        [Required(ErrorMessage = "Cpdb is required")]
        public float cPdb { get; set; }

        [Required(ErrorMessage = "A is required")]
        public float a { get; set; }

        [Required(ErrorMessage = "Amax is required")]
        public int aMax { get; set; }

        [Required(ErrorMessage = "D is required")]
        public int d { get; set; }

        [Required(ErrorMessage = "Aprime is required")]
        public int aPrime { get; set; }

        [Required(ErrorMessage = "Ccb is required")]
        [Display(Name = "")]
        public float cCb { get; set; }

        [Required(ErrorMessage = "M1 is required")]
        public float m1 { get; set; }

        [Required(ErrorMessage = "P0 is required")]
        public int p0 { get; set; }

        [Required(ErrorMessage = "M2 is required")]
        public float m2 { get; set; }

        [Required(ErrorMessage = "Ceb is required")]
        public float cEb { get; set; }

        [Required(ErrorMessage = "S is required")]
        public int s { get; set; }

        [Required(ErrorMessage = "M3 is required")]
        public float m3 { get; set; }

        [Required(ErrorMessage = "Se is required")]
        public short sE { get; set; }

        [Required(ErrorMessage = "M4 is required")]
        public float m4 { get; set; }

        [Required(ErrorMessage = "Sh is required")]
        public short sH { get; set; }

        [Required(ErrorMessage = "M5 is required")]
        public float m5 { get; set; }

        public string message { get; set; }
    }

    public class RejectedModel
    {
        public List<RejectedTransactionModel> rejected { get; set; }
    }

    public class RejectedTransactionModel
    {
        public Guid transactionID { get; set; }
        public string name { get; set; }
        public int gateway { get; set; }
        public DateTime? dateRejected { get; set; }
        public string producer { get; set; }
        public string consumer { get; set; }
    }

    public class ClassOverviewModel
    {
        public List<ClassModel> products { get; set; }
        public List<ClassModel> productsDefault { get; set; }
        //public Dictionary<string, int> ItemClassTemplates { get; set; }
        public List<SelectListItem> ItemClassTemplates { get; set; }
    }

    public class ClassModel
    {
        public int classID { get; set; }
        public string className { get; set; }
        public string productName { get; set; }
        public string productDesc { get; set; }
        public Guid productGuid { get; set; }
        public string productOwner { get; set; }
    }

}
