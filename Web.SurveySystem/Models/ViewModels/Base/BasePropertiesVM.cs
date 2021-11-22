using System;
using System.ComponentModel.DataAnnotations;

namespace Web.SurveySystem.Models.ViewModels.Base
{
    public class BasePropertiesVM
    {
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}