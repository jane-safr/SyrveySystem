using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("Surveys", Schema = "Survey")]
    public class Survey : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid SurveyId { get; set; }
        [Index]
        public int SurveyCode { get; set; }

        [Required] // тип
        public Guid SurveyTypeId { get; set; }
        public SurveyType SurveyType { get; set; }

        [Required]
        [StringLength(1000)]
        public string NameRus { get; set; }
        [Required]
        [StringLength(1000)]
        public string NameEng { get; set; }
        [Required]
        [StringLength(2000)]
        public string PurposeRus { get; set; }
        [Required]
        [StringLength(2000)]
        public string PurposeEng { get; set; }

        [StringLength(500)]
        public string ShortLink { get; set; } // короткая ссылка анон теста

        [Required]
        public int TimeEstimateMin { get; set; } // рек. время прохождения в мин
        [Required]
        public bool IsRandomQuestions { get; set; }
        [Required]
        public bool IsAnonymous { get; set; }
        public ICollection<Invitation> Invitations { get; set; }
        public ICollection<Question> Questions { get; set; }
        public Survey()
        {
            Invitations = new List<Invitation>();
            Questions = new List<Question>();
        }
    }
}