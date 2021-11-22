using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class SurveyTypeVM : BasePropertiesVM
    {
        public Guid? SurveyTypeId { get; set; }
        [Required(ErrorMessage = "Укажите наименование (рус.) / Enter Name Rus")]
        [StringLength(200, ErrorMessage = "Наименование (рус.) от {2} до {1} символов / Name Rus from {2} to {1} letters", MinimumLength = 3)]
        public string NameRus { get; set; }
        [Required(ErrorMessage = "Укажите наименование (англ.) / Enter Name Eng")]
        [StringLength(200, ErrorMessage = "Наименование (англ.) от {2} до {1} символов / Name Eng from {2} to {1} letters", MinimumLength = 3)]
        public string NameEng { get; set; }
        [Required(ErrorMessage = "Укажите описание (рус.) / Enter Description Rus")]
        [StringLength(200, ErrorMessage = "Описание (рус.) от {2} до {1} символов / Description Rus from {2} to {1} letters", MinimumLength = 3)]
        public string DescriptionRus { get; set; }
        [Required(ErrorMessage = "Укажите описание (англ.) / Enter Description Eng")]
        [StringLength(200, ErrorMessage = "Описание (англ.) от {2} до {1} символов / Description Eng from {2} to {1} letters", MinimumLength = 3)]
        public string DescriptionEng { get; set; }
        public string FullName { get; set; }
        public ICollection<SurveyVM> Surveys { get; set; }
        public SurveyTypeVM()
        {
            Surveys = new List<SurveyVM>();
        }
    }
}