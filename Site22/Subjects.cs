namespace Site22
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Subjects
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        public float? Coef { get; set; }

        public int? ID_employee { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public virtual Employee Employee { get; set; }
    }
}