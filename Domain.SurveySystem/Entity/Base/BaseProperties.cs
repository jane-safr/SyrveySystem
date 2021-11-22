using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.SurveySystem.Entity.Base
{
    public class BaseProperties
    {
        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        [Required] [StringLength(500)]
        public string CreatedBy { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}