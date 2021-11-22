using System;
using System.Collections.Generic;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class InvitationVM : BasePropertiesVM
    {
        public Guid InvitationId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime DateEnd { get; set; } // дата окончания прохождения
        public bool IsAccepted { get; set; } // начал ли пользователь проходить тест
        public bool IsFinished { get; set; } // тест завершен данным пользователем
        public int Percent { get; set; } // процент прохождения
        public DateTime? DateStart { get; set; } // дата начала (accept)
        public int InvitationCode { get; set; }
        public Guid SurveyId { get; set; }
        public SurveyVM Survey { get; set; }
        public DateTime? ActualCompleteDate { get; set; } // дата окончания прохождения (фактическая)
        public ICollection<UserAnswerVM> UserAnswers { get; set; }
        public InvitationVM()
        {
            UserAnswers = new List<UserAnswerVM>();
        }
    }
}