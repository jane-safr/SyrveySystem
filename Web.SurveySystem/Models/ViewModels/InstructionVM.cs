using System;
using System.ComponentModel.DataAnnotations;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class InstructionVM : BasePropertiesVM
    {
        public Guid? InstructionId { get; set; }

        [Required(ErrorMessage = "Введите NameRus / Enter NameRus")]
        [StringLength(3000, ErrorMessage = "NameRus должен быть от {2} до {1} символов", MinimumLength = 1)]
        public string NameRus { get; set; }
        [Required(ErrorMessage = "Введите NameEng / Enter NameEng")]
        [StringLength(3000, ErrorMessage = "NameEng должен быть от {2} до {1} символов", MinimumLength = 1)]
        public string NameEng { get; set; }
        public Guid? UploadFileRusId { get; set; }
        public Guid? UploadFileEngId { get; set; }
        [Required]
        public bool IsAdmin { get; set; }
        public int? Code { get; set; } // hash (guid)
    }
}