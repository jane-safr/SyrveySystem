using System;
using System.Collections.Generic;
using BLL.SurveySystem.DTO.Base;

namespace BLL.SurveySystem.DTO
{
   public class QuestionDTO : BasePropertiesDTO
    {
        public Guid QuestionId { get; set; }
        public string QuestionRus { get; set; }
        public string QuestionEng { get; set; }
        public bool IsCriterion { get; set; }
        public Guid? IndicatorId { get; set; }
        public IndicatorDTO Indicator { get; set; }
        public bool IsInReport { get; set; }
        public Guid SurveyId { get; set; }
        public SurveyDTO Survey { get; set; }
        public Guid QuestionTypeId { get; set; }
        public QuestionTypeDTO QuestionType { get; set; }
        public int Group { get; set; } // порядок
        public ICollection<AnswerDTO> Answers { get; set; }
        public QuestionDTO()
        {
            Answers = new List<AnswerDTO>();
        }
    }
}
