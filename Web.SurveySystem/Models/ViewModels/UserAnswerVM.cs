using System;
using System.ComponentModel.DataAnnotations;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class UserAnswerVM : BasePropertiesVM
    {
        public Guid? UserAnswerId { get; set; }
        public InvitationVM Invitation { get; set; }
        public Guid InvitationId { get; set; }
        [Required]
        public Guid QuestionId { get; set; }
        public Guid? AnswerId { get; set; }
        public bool IsValid { get; set; }
        public int Order { get; set; }
        public string UserAnswerText { get; set; }

    }
}