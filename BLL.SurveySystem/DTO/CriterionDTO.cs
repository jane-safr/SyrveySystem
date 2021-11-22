using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class CriterionDTO : BasePropertiesDTO
    {
        public Guid CriterionId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }

        public ICollection<ParameterDTO> Parameters { get; set; }
        public CriterionDTO()
        {
            Parameters = new List<ParameterDTO>();
        }
    }
}
