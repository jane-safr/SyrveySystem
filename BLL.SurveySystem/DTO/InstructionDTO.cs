using System;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class InstructionDTO : BasePropertiesDTO
    {
        public Guid InstructionId { get; set; }
        public string NameRus { get; set; }
        public string NameEng { get; set; }
        public Guid? UploadFileRusId { get; set; }
        public Guid? UploadFileEngId { get; set; }
        public bool IsAdmin { get; set; }
        public int Code { get; set; } // hash (guid)
    }
}
