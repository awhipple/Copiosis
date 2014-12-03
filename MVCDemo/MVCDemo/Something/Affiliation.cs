namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Affiliation")]
    public partial class Affiliation
    {
        public Affiliation()
        {
            AffiliationRels = new HashSet<AffiliationRel>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int aff_id { get; set; }

        [StringLength(20)]
        public string title { get; set; }

        [StringLength(50)]
        public string description { get; set; }

        public virtual ICollection<AffiliationRel> AffiliationRels { get; set; }
    }
}
