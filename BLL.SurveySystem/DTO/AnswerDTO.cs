using System;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
   public class AnswerDTO : BasePropertiesDTO
    {
        public Guid AnswerId { get; set; }
        public string AnswerRus { get; set; }
        public string AnswerEng { get; set; }
        public bool IsValid { get; set; }
        public int Credit { get; set; } // баллы по умолч
        public Guid QuestionId { get; set; }
        public QuestionDTO Question { get; set; }
    }
}