using Domain.SurveySystem.Entity.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.SurveySystem.Entity
{
    [Table("Instructions", Schema = "Resource")]
    public class Instruction : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid InstructionId { get; set; }

        [Required]
        [StringLength(400)]
        public string NameRus { get; set; }

        [Required]
        [StringLength(400)]
        public string NameEng { get; set; }

        [Index]
        public Guid? UploadFileRusId { get; set; }

        [Index]
        public Guid? UploadFileEngId { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        [Required]
        [Index]
        public int Code { get; set; } // hash (guid)
    }
}
