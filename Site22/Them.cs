namespace Site22
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Thems")]
    public partial class Them
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public int? ID_Employee { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public virtual Employee Employee { get; set; }
    }
}
