using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("FixedAnswers", Schema = "QA")]
    public class FixedAnswer : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid FixedAnswerId { get; set; }
        [Required]
        [StringLength(3000)]
        public string FixAnswerRus { get; set; }
        [Required]
        [StringLength(3000)]
        public string FixAnswerEng { get; set; }
        [Required]
        public int Credit { get; set; } // баллы по умолч
        [Required]
        public Guid QuestionTypeId { get; set; }
        public QuestionType QuestionType { get; set; }
    }
}
