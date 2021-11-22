using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
    public class QuestionTypeDTO : BasePropertiesDTO
    {
      public Guid QuestionTypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsFixedAnswer { get; set; }
        public bool IsOpenAnswer { get; set; }
        public ICollection<QuestionDTO> Questions { get; set; }
        public ICollection<FixedAnswerDTO> FixedAnswers { get; set; }
        public string Comment { get; set; }
        public QuestionTypeDTO()
        {
            Questions = new List<QuestionDTO>();
            FixedAnswers = new List<FixedAnswerDTO>();
        }
    }
}