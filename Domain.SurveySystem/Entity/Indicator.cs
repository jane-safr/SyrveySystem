using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("Indicators", Schema = "Criterion")]
    public class Indicator : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid IndicatorId { get; set; }
        [Required]
        [StringLength(3000)]
        public string Name { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public Guid ParameterId { get; set; }
        public Parameter Parameter { get; set; }

        public ICollection<Question> Questions { get; set; }
        public Indicator()
        {
            Questions = new List<Question>();
        }
    }
}
