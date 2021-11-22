using System;

namespace BLL.SurveySystem.DTO
{
   public class SettingDTO
    {
        public Guid SettingId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}