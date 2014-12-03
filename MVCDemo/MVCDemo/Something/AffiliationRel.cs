namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AffiliationRel")]
    public partial class AffiliationRel
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int aff_id { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int agent_id { get; set; }

        [StringLength(10)]
        public string affiliation_strength { get; set; }

        public virtual Affiliation Affiliation { get; set; }

        public virtual Agent Agent { get; set; }
    }
}
