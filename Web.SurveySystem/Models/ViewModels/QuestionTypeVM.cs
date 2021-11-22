using System;
using System.Collections.Generic;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class QuestionTypeVM : BasePropertiesVM
    {
        public Guid? QuestionTypeId { get; set; }
        public string TypeName { get; set; }
        public bool IsFixedAnswer { get; set; }
        public bool IsOpenAnswer { get; set; }
        public ICollection<QuestionVM> Questions { get; set; }
        public ICollection<FixedAnswerVM> FixedAnswers { get; set; }
        public string Comment { get; set; }
        public QuestionTypeVM()
        {
            Questions = new List<QuestionVM>();
            FixedAnswers = new List<FixedAnswerVM>();
        }
    }
}