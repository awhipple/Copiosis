using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;
using Copiosis_Application.DB_Data;

namespace Copiosis_Application.Models
{
    public class AddClassModel
    {
        
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
}
