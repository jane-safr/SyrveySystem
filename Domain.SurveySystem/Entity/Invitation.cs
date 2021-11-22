using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("Invitations", Schema = "Survey")]
    public class Invitation : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid InvitationId { get; set; }
        [Required]
        [Index]
        public Guid UserId { get; set; }
        [Required]
        [StringLength(500)]
        public string UserName { get; set; }
        [Required]
        [StringLength(300)]
        public string UserEmail { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime DateEnd { get; set; } // дата окончания прохождения
        [Required]
        public bool IsAccepted { get; set; } // начал ли пользователь проходить тест
        [Required]
        public bool IsFinished { get; set; } // тест завершен данным пользователем
        [Required]
        public int Percent { get; set; } // процент прохождения
        [Column(TypeName = "datetime2")]
        public DateTime? DateStart { get; set; } // дата начала (accept)
        [Index]
        public int InvitationCode { get; set; }
        [Required] 
        public Guid SurveyId { get; set; }
        public Survey Survey { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? ActualCompleteDate { get; set; } // дата окончания прохождения (фактическая)
        public ICollection<UserAnswer> UserAnswers { get; set; }
        public Invitation()
        {
            UserAnswers = new List<UserAnswer>();
        }
    }
}
