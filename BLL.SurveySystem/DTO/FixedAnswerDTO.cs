using System;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class FixedAnswerDTO : BasePropertiesDTO
    {
        public Guid FixedAnswerId { get; set; }
        public string FixAnswerRus { get; set; }
        public string FixAnswerEng { get; set; }
        public int Credit { get; set; } // баллы по умолч
        public Guid QuestionTypeId { get; set; }
        public QuestionTypeDTO QuestionType { get; set; }
    }
}