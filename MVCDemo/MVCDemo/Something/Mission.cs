namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Mission")]
    public partial class Mission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int mission_id { get; set; }

        [StringLength(20)]
        public string name { get; set; }

        public int? access_id { get; set; }

        public int? team_id { get; set; }

        [StringLength(20)]
        public string mission_status { get; set; }

        public virtual SecurityClearance SecurityClearance { get; set; }

        public virtual Team Team { get; set; }
    }
}
