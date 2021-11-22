using System;
using System.Collections.Generic;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class IndicatorVM : BasePropertiesVM
    {
        public Guid? IndicatorId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid ParameterId { get; set; }
        public ParameterVM Parameter { get; set; }
        public ICollection<QuestionVM> Questions { get; set; }
        public IndicatorVM()
        {
            Questions = new List<QuestionVM>();
        }
        public string FullName { get; set; }
        public string FullNumber { get; set; }
    }
}