//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Copiosis_Application.DB_Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class location
    {
        public location()
        {
            this.users = new HashSet<user>();
        }
    
        public int locationID { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string neighborhood { get; set; }
        public string signupKey { get; set; }
    
        public virtual ICollection<user> users { get; set; }
    }
}
