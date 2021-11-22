using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("UserAnswers", Schema = "Respondent")]
    public class UserAnswer : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid UserAnswerId { get; set; }
        [Required]
        public Invitation Invitation { get; set; }
        public Guid InvitationId { get; set; }
        [Index]
        public Guid QuestionId { get; set; }
        public Guid? AnswerId { get; set; } // null тк при нарезке вопросов - еще не заполнен
        public bool IsValid { get; set; }
        [Required]
        public int Order { get; set; }
        [StringLength(1500)]
        public string UserAnswerText { get; set; } 
    }
}