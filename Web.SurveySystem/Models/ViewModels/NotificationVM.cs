using System;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class NotificationVM : BasePropertiesVM
    {
        public Guid NotificationId { get; set; }
        public string EmailTo { get; set; }
        public string EmailText { get; set; }
        public string EmailUrl { get; set; }
        public DateTime? DateSend { get; set; }
        public bool IsSend { get; set; }
        public Guid Id { get; set; }
        public Guid NotificationTypeId { get; set; }
        public NotificationTypeVM NotificationType { get; set; }
    }
}