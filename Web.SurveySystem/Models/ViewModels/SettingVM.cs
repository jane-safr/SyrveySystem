using System;
using System.ComponentModel.DataAnnotations;

namespace Web.SurveySystem.Models.ViewModels
{
    public class SettingVM
    {
        public Guid? SettingId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Value { get; set; }
        [Required]
        public string Description { get; set; }
    }
}