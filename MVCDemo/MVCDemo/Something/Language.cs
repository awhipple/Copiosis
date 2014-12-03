namespace MVCDemo.Something
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Language")]
    public partial class Language
    {
        public Language()
        {
            Agents = new HashSet<Agent>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int lang_id { get; set; }

        [Column("language")]
        [StringLength(20)]
        public string language1 { get; set; }

        public virtual ICollection<Agent> Agents { get; set; }
    }
}
