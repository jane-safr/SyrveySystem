using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.SurveySystem.Entity
{
    [Table("Settings", Schema = "Setting")]
    public class Setting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SettingId { get; set; }

        [Required]
        [StringLength(400)] // length Nvarchar for nonclustered index  = 1700b. (Name,Value)
        public string Name { get; set; }

        [Required]
        [StringLength(400)]
        public string Value { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }
    }
}