using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.SurveySystem.Entity.Base;

namespace Domain.SurveySystem.Entity
{
    [Table("UploadedFiles", Schema = "Resource")]
    public class UploadedFile : BaseProperties
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid UploadedFileId { get; set; }

        [Required]
        [Index]
        public Guid ApplicationUserId { get; set; }

        [Required]
        [StringLength(3000)]
        public string PathFile { get; set; }

        [StringLength(1000)]
        public string FileName { get; set; }

        public int FileSize { get; set; }

        [Required]
        //Тип файла 1 - документы, 2 - инструкции Rus, 3 - инструкции Eng
        public int FileType { get; set; }
    }
}