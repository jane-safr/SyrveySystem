using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("QuestionTypes", Schema = "QA")]
    public class QuestionType : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid QuestionTypeId { get; set; }

        [Required]
        [StringLength(2000)]
        public string TypeName { get; set; }

        [Required]
        public bool IsFixedAnswer { get; set; }
        [Required]
        public bool IsOpenAnswer { get; set; } // ответ в свободной форме (не из списка)
        public string Comment { get; set; }

        public ICollection<Question> Questions { get; set; }
        public ICollection<FixedAnswer> FixedAnswers { get; set; }
        public QuestionType()
        {
            Questions = new List<Question>();
            FixedAnswers = new List<FixedAnswer>();
        }
    }
}
