using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class IndicatorDTO : BasePropertiesDTO
    {
        public Guid IndicatorId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid ParameterId { get; set; }
        public ParameterDTO Parameter { get; set; }
        public ICollection<QuestionDTO> Questions { get; set; }

        public IndicatorDTO()
        {
            Questions = new List<QuestionDTO>();
        }
    }
}