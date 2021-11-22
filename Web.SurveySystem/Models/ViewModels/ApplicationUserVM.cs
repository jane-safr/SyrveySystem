using Microsoft.AspNet.Identity.EntityFramework;

namespace Web.SurveySystem.Models.ViewModels
{
    public class ApplicationUserVM : IdentityUser
    {
        public bool IsExternal { get; set; }
        public string DisplayName { get; set; }
    }
}