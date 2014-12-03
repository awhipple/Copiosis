namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Team")]
    public partial class Team
    {
        public Team()
        {
            Missions = new HashSet<Mission>();
            Agents = new HashSet<Agent>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int team_id { get; set; }

        [StringLength(20)]
        public string name { get; set; }

        [StringLength(20)]
        public string meeting_frequency { get; set; }

        public virtual ICollection<Mission> Missions { get; set; }

        public virtual ICollection<Agent> Agents { get; set; }
    }
}
