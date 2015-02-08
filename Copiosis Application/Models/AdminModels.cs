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
        [Display(Name = "Name")]
        public string name { get; set; }

        [Required(ErrorMessage = "SuggestedGateway is required")]
        [Display(Name = "Suggested Gateway")]
        public int suggestedGateway { get; set; }

        [Required(ErrorMessage = "Cpdb is required")]
        [Display(Name = "Cpdb")]
        public float cPdb { get; set; }

        [Required(ErrorMessage = "A is required")]
        [Display(Name = "Resource abundance")]
        public float a { get; set; }

        [Required(ErrorMessage = "Amax is required")]
        [Display(Name = "Max resources")]
        public int aMax { get; set; }

        [Required(ErrorMessage = "D is required")]
        [Display(Name = "Consumer demand")]
        public int d { get; set; }

        [Required(ErrorMessage = "Aprime is required")]
        [Display(Name = "Producer population")]
        public int aPrime { get; set; }

        [Required(ErrorMessage = "Ccb is required")]
        [Display(Name = "Ccb")]
        public float cCb { get; set; }

        [Required(ErrorMessage = "M1 is required")]
        [Display(Name = "M1")]
        public float m1 { get; set; }

        [Required(ErrorMessage = "PO is required")]
        [Display(Name = "Consumer objective benefit")]
        public int pO { get; set; }

        [Required(ErrorMessage = "M2 is required")]
        [Display(Name = "M2")]
        public float m2 { get; set; }

        [Required(ErrorMessage = "Ceb is required")]
        [Display(Name = "Ceb")]
        public float cEb { get; set; }

        [Required(ErrorMessage = "S is required")]
        [Display(Name = "Social benefit")]
        public int s { get; set; }

        [Required(ErrorMessage = "M3 is required")]
        [Display(Name = "M3")]
        public float m3 { get; set; }

        [Required(ErrorMessage = "Se is required")]
        [Display(Name = "Environmental impact")]
        public short sE { get; set; }

        [Required(ErrorMessage = "M4 is required")]
        [Display(Name = "M4")]
        public float m4 { get; set; }

        [Required(ErrorMessage = "Sh is required")]
        [Display(Name = "Human impact")]
        public short sH { get; set; }

        [Required(ErrorMessage = "M5 is required")]
        [Display(Name = "M5")]
        public float m5 { get; set; }

        public string message { get; set; }

        public AddClassModel()
        {
            suggestedGateway = 1;
            cPdb = 1;
            a = 1;
            aMax = 1;
            d = 1;
            aPrime = 1;
            cCb = 1;
            m1 = 1;
            pO = 1;
            m2 = 1;
            cEb = 1;
            s = 1;
            m3 = 1;
            sE = 1;
            m4 = 1;
            sH = 1;
            m5 = 1;
        }

        public bool Equals(itemClass itemClass)
        {
            return itemClass.name == this.name && itemClass.suggestedGateway == this.suggestedGateway && itemClass.cPdb == this.cPdb && itemClass.a == this.a && itemClass.aMax == this.aMax && itemClass.d == this.d &&
                itemClass.aPrime == this.aPrime && itemClass.cCb == this.cCb && itemClass.m1 == this.m1 && itemClass.pO == this.pO && itemClass.m2 == this.m2 && itemClass.cEb == this.cEb &&
                itemClass.s == this.s && itemClass.m3 == this.m3 && itemClass.sE == this.sE && itemClass.m4 == this.m4 && itemClass.sH == this.sH && itemClass.m5 == this.m5;
        }
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


    public class ViewUsersModel
    {
        public List<UserModel> adminUsers { get; set; }
        public List<UserModel> nonadminUsers { get; set; }
    }

    public class UserModel
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int status { get; set; }
        public int roleId { get; set; }
        public string roleName { get; set; }

    }

    public class ViewClassesModel
    {
        public List<ViewClassModel> ItemClassTemplates { get; set; }
    }

    public class ViewClassModel
    {
        public int classID { get; set; }
        public string className { get; set; }
        public int numUsing { get; set; }
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
