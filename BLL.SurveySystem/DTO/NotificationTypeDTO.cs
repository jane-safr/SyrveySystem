using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class NotificationTypeDTO : BasePropertiesDTO
    {
        public Guid NotificationTypeId { get; set; }
        public string NameRus { get; set; }
        public string NameEng { get; set; }
        public string MessageTemplate { get; set; }
        public string TemplateLink { get; set; }
        public bool IsSurvey { get; set; } // анкета
        public bool IsAnonymousSurvey { get; set; } // анонимный тип
        public bool IsTest { get; set; } // тест как проверка знаний

        public ICollection<NotificationDTO> Notifications { get; set; }
        public NotificationTypeDTO()
        {
            Notifications = new List<NotificationDTO>();
        }
    }
}