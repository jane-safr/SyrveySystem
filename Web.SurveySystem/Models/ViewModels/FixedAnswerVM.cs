using System;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class FixedAnswerVM : BasePropertiesVM
    {
        public Guid? FixedAnswerId { get; set; }
        public string FixAnswerRus { get; set; }
        public string FixAnswerEng { get; set; }
        public int Credit { get; set; } // баллы по умолч
        public Guid QuestionTypeId { get; set; }
        public QuestionTypeVM QuestionType { get; set; }
    }
}