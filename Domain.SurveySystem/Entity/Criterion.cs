using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("Criterions", Schema = "Criterion")]
    public class Criterion : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid CriterionId { get; set; }
        [Required]
        [StringLength(2000)]
        public string Name { get; set; }
        [Required]
        public int Order { get; set; }

        public ICollection<Parameter> Parameters { get; set; }
        public Criterion()
        {
            Parameters = new List<Parameter>();
        }
    }
}
