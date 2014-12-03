namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Agent")]
    public partial class Agent
    {
        public Agent()
        {
            AffiliationRels = new HashSet<AffiliationRel>();
            Languages = new HashSet<Language>();
            Skills = new HashSet<Skill>();
            Teams = new HashSet<Team>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int agent_id { get; set; }

        [StringLength(20)]
        public string first { get; set; }

        [StringLength(20)]
        public string middle { get; set; }

        [StringLength(20)]
        public string last { get; set; }

        [StringLength(50)]
        public string address { get; set; }

        [StringLength(20)]
        public string city { get; set; }

        [StringLength(20)]
        public string country { get; set; }

        public int? salary { get; set; }

        public int? clearance_id { get; set; }

        public virtual ICollection<AffiliationRel> AffiliationRels { get; set; }

        public virtual SecurityClearance SecurityClearance { get; set; }

        public virtual ICollection<Language> Languages { get; set; }

        public virtual ICollection<Skill> Skills { get; set; }

        public virtual ICollection<Team> Teams { get; set; }
    }
}
