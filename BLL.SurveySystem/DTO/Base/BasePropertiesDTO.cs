using System;

namespace BLL.SurveySystem.DTO.Base
{
    public class BasePropertiesDTO
    {
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}