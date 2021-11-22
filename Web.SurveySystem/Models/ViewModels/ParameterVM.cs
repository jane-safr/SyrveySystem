using System;
using System.Collections.Generic;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class ParameterVM : BasePropertiesVM
    {
        public Guid? ParameterId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid CriterionId { get; set; }
        public string FullName { get; set; }
        public virtual CriterionVM Criterion { get; set; }

        public ICollection<IndicatorVM> Indicators { get; set; }
        public ParameterVM()
        {
            Indicators = new List<IndicatorVM>();
        }
    }
}