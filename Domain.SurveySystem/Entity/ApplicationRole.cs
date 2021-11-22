using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Domain.SurveySystem.Entity
{
    public class ApplicationRole : IdentityRole
    {
        [StringLength(1000)]
        public string Description { get; set; }
        public bool IsShown { get; set; }
    }
}