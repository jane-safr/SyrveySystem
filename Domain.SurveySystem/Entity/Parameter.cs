using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
   [Table("Parameters", Schema = "Criterion")]
   public class Parameter : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid ParameterId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Name { get; set; }
        [Required]
        public int Order { get; set; }

        [Required]
        public Guid CriterionId { get; set; }
        public virtual Criterion Criterion { get; set; }
        
        public ICollection<Indicator> Indicators { get; set; }
        public Parameter()
        {
            Indicators = new List<Indicator>();
        }
    }
}
