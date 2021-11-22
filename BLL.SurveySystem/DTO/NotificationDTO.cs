using System;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class NotificationDTO : BasePropertiesDTO
    {
        public Guid NotificationId { get; set; }
        public string EmailTo { get; set; }
        public string EmailText { get; set; }
        public string EmailUrl { get; set; }
        public DateTime? DateSend { get; set; }
        public bool IsSend { get; set; }
        public Guid Id { get; set; } // Id приглашения или теста
        public Guid NotificationTypeId { get; set; }
        public NotificationTypeDTO NotificationType { get; set; }
    }
}