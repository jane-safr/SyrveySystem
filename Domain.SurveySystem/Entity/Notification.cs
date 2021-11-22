using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("Notifications", Schema = "Setting")]
    public class Notification : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid NotificationId { get; set; }

        [Required]
        public string EmailTo { get; set; }

        [Required]
        public string EmailText { get; set; }

        [StringLength(1500)]
        public string EmailUrl { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateSend { get; set; }

        [Required]
        public bool IsSend { get; set; }

        [Required]
        [Index]
        public Guid Id { get; set; } // Id приглашения или теста

        [Required]
        public Guid NotificationTypeId { get; set; }

        public NotificationType NotificationType { get; set; }
    }
}
