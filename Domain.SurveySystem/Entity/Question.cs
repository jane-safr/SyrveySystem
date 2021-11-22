using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("Questions", Schema = "QA")]
    public class Question : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid QuestionId { get; set; }
        [Required]
        [StringLength(3000)]
        public string QuestionRus { get; set; }
        [Required]
        [StringLength(3000)]
        public string QuestionEng { get; set; }
        [Required]
        public bool IsCriterion { get; set; }
        public Guid? IndicatorId { get; set; }
        public Indicator Indicator { get; set; }
        [Required]
        public bool IsInReport { get; set; }
        [Required]
        public Guid SurveyId { get; set; }
        public Survey Survey { get; set; }
        [Required]
        public Guid QuestionTypeId { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Group { get; set; } // порядок
        public ICollection<Answer> Answers { get; set; }
        public Question()
        {
            Answers = new List<Answer>();
        }
    }
}
