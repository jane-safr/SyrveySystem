using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("Answers", Schema = "QA")]
    public class Answer : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid AnswerId { get; set; }
        [Required]
        [StringLength(3000)]
        public string AnswerRus { get; set; }
        [Required]
        [StringLength(3000)]
        public string AnswerEng { get; set; }
        [Required]
        public bool IsValid { get; set; }
        [Required]
        public int Credit { get; set; } // баллы по умолч
        [Required]
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
