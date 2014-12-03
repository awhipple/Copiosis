namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Skill")]
    public partial class Skill
    {
        public Skill()
        {
            Agents = new HashSet<Agent>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int skill_id { get; set; }

        [Column("skill")]
        [StringLength(20)]
        public string skill1 { get; set; }

        public virtual ICollection<Agent> Agents { get; set; }
    }
}
