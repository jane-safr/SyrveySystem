using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("NotificationTypes", Schema = "Setting")]
    public class NotificationType : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid NotificationTypeId { get; set; }

        [Required]
        [StringLength(2000)]
        public string NameRus { get; set; }

        [Required]
        [StringLength(2000)]
        public string NameEng { get; set; }

        [Required]
        public string MessageTemplate { get; set; }

        [StringLength(500)]
        public string TemplateLink { get; set; }

        [Required]
        public bool IsSurvey { get; set; } // анкета
        [Required]
        public bool IsAnonymousSurvey { get; set; } // анонимный тип

        [Required]
        public bool IsTest { get; set; } // тест как проверка знаний
        
        public ICollection<Notification> Notifications { get; set; }
        public NotificationType()
        {
            Notifications = new List<Notification>();
        }
    }
}
