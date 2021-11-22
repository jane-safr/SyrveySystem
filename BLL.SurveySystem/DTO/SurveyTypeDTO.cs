using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class SurveyTypeDTO : BasePropertiesDTO
    {
        public Guid SurveyTypeId { get; set; }
        public string NameRus { get; set; }
        public string NameEng { get; set; }
        public string DescriptionRus { get; set; }
        public string DescriptionEng { get; set; }
        public ICollection<SurveyDTO> Surveys { get; set; }
        public SurveyTypeDTO()
        {
            Surveys = new List<SurveyDTO>();
        }
    }
}