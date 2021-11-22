using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class SurveyDTO : BasePropertiesDTO
    {
        public Guid SurveyId { get; set; }
        public int SurveyCode { get; set; }
        public Guid SurveyTypeId { get; set; }
        public SurveyTypeDTO SurveyType { get; set; }
        public string NameRus { get; set; }
        public string NameEng { get; set; }
        public string PurposeRus { get; set; }
        public string PurposeEng { get; set; }
        public int TimeEstimateMin { get; set; } // рек. время прохождения в мин
        public bool IsRandomQuestions { get; set; }
        public bool IsAnonymous { get; set; }
        public string ShortLink { get; set; } // короткая ссылка анон теста
        public ICollection<InvitationDTO> Invitations { get; set; }
        public ICollection<QuestionDTO> Questions { get; set; }
        public SurveyDTO()
        {
            Invitations = new List<InvitationDTO>();
            Questions = new List<QuestionDTO>();
        }
    }
}
