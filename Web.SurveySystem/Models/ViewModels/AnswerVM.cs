using System;
using Web.SurveySystem.Models.ViewModels.Base;

namespace Web.SurveySystem.Models.ViewModels
{
    public class AnswerVM : BasePropertiesVM
    {
        public Guid? AnswerId { get; set; }
        public string AnswerRus { get; set; }
        public string AnswerEng { get; set; }
        public bool IsValid { get; set; }
        public int Credit { get; set; } // баллы по умолч
        public Guid QuestionId { get; set; }
        public QuestionVM Question { get; set; }
    }
}