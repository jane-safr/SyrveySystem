using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("SurveyTypes", Schema = "Survey")]
    public class SurveyType : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid SurveyTypeId { get; set; }

        [Required]
        [StringLength(1000)]
        public string NameRus { get; set; }
        [Required]
        [StringLength(1000)]
        public string NameEng { get; set; }

        [Required]
        [StringLength(1000)]
        public string DescriptionRus { get; set; }
        [Required]
        [StringLength(1000)]
        public string DescriptionEng { get; set; }

        public ICollection<Survey> Surveys { get; set; }

        public SurveyType()
        {
            Surveys = new List<Survey>();
        }
    }
}
