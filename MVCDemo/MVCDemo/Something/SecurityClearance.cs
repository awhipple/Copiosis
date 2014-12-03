namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SecurityClearance")]
    public partial class SecurityClearance
    {
        public SecurityClearance()
        {
            Agents = new HashSet<Agent>();
            Missions = new HashSet<Mission>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int sc_id { get; set; }

        [StringLength(20)]
        public string sc_level { get; set; }

        [StringLength(50)]
        public string description { get; set; }

        public virtual ICollection<Agent> Agents { get; set; }

        public virtual ICollection<Mission> Missions { get; set; }
    }
}
