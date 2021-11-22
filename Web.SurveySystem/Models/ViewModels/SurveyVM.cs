using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class SurveyVM : BasePropertiesVM
    {
        public Guid? SurveyId { get; set; }
        public int SurveyCode { get; set; }
        [Required(ErrorMessage = "Выберите тип / Select type")]
        public Guid SurveyTypeId { get; set; }
        public SurveyTypeVM SurveyType { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Наименование (рус.) от {2} до {1} символов / Name Rus from {2} to {1} letters", MinimumLength = 3)]
        public string NameRus { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Наименование (англ.) от {2} до {1} символов / Name Eng from {2} to {1} letters", MinimumLength = 3)]
        public string NameEng { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Цель (рус.) от {2} до {1} символов / Purpose Rus from {2} to {1} letters", MinimumLength = 3)]
        public string PurposeRus { get; set; }
        [Required]
        [StringLength(200, ErrorMessage = "Цель (англ.) от {2} до {1} символов / Purpose Eng from {2} to {1} letters", MinimumLength = 3)]
        public string PurposeEng { get; set; }
        [Required]
        [Range(1, 1440, ErrorMessage = "Время от {1} до {2} мин. / Estimated time from {2} to {1} mins")]
        public int TimeEstimateMin { get; set; } // рек. время прохождения в мин
        [Required]
        public bool IsRandomQuestions { get; set; }
        [Required]
        public bool IsAnonymous { get; set; }

        [StringLength(500, ErrorMessage = "Ссылка от {2} до {1} символов / Link from {2} to {1} letters", MinimumLength = 10)]
        public string ShortLink { get; set; } // короткая ссылка анон теста
        public string FullName { get; set; }
        public ICollection<InvitationVM> Invitations { get; set; }
        public ICollection<QuestionVM> Questions { get; set; }
        public SurveyVM()
        {
            Invitations = new List<InvitationVM>();
            Questions = new List<QuestionVM>();
        }
    }
}