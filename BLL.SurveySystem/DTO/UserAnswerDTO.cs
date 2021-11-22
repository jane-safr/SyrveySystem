using System;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class UserAnswerDTO : BasePropertiesDTO
    {
        public Guid UserAnswerId { get; set; }
        public InvitationDTO Invitation { get; set; }
        public Guid InvitationId { get; set; }
        public Guid QuestionId { get; set; }
        public Guid? AnswerId { get; set; }
        public bool IsValid { get; set; }
        public int Order { get; set; }
        public string UserAnswerText { get; set; }
    }
}