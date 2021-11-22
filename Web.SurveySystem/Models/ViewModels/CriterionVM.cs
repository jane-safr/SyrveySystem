using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class CriterionVM : BasePropertiesVM
    {
        public Guid? CriterionId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Order { get; set; }
        public string FullName { get; set; }

        public ICollection<ParameterVM> Parameters { get; set; }
        public CriterionVM()
        {
            Parameters = new List<ParameterVM>();
        }
    }
}