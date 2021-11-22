using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class ParameterDTO : BasePropertiesDTO
    {
        public Guid ParameterId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid CriterionId { get; set; }
        public virtual CriterionDTO Criterion { get; set; }

        public ICollection<IndicatorDTO> Indicators { get; set; }
        public ParameterDTO()
        {
            Indicators = new List<IndicatorDTO>();
        }
    }
}
