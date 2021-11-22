using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class InvitationDTO : BasePropertiesDTO
    {
        public Guid InvitationId { get; set; }
        public int InvitationCode { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime DateEnd { get; set; } // дата окончания прохождения
        public bool IsAccepted { get; set; } // начал ли пользователь проходить тест
        public bool IsFinished { get; set; } // тест завершен данным пользователем
        public int Percent { get; set; } // процент прохождения
        public DateTime? DateStart { get; set; } // дата начала (accept)
        public Guid SurveyId { get; set; }
        public SurveyDTO Survey { get; set; }
        public DateTime? ActualCompleteDate { get; set; } // дата окончания прохождения (фактическая)
        public ICollection<UserAnswerDTO> UserAnswers { get; set; }
        public InvitationDTO()
        {
            UserAnswers = new List<UserAnswerDTO>();
        }
    }
}
