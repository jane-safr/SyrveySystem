using System;
using System.Collections.Generic;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class QuestionVM : BasePropertiesVM
    {
        public Guid? QuestionId { get; set; }
        public string QuestionRus { get; set; }
        public string QuestionEng { get; set; }
        public bool IsCriterion { get; set; }
        public Guid? IndicatorId { get; set; }
        public IndicatorVM Indicator { get; set; }
        public bool IsInReport { get; set; }
        public Guid SurveyId { get; set; }
        public SurveyVM Survey { get; set; }
        public Guid QuestionTypeId { get; set; }
        public QuestionTypeVM QuestionType { get; set; }
        public int Group { get; set; } // порядок
        public ICollection<AnswerVM> Answers { get; set; }
        public QuestionVM()
        {
            Answers = new List<AnswerVM>();
        }
    }
}