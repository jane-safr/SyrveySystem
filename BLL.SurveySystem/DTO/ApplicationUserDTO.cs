using Microsoft.AspNet.Identity.EntityFramework;

namespace BLL.SurveySystem.DTO
{
   public class ApplicationUserDTO : IdentityUser
    {
        public bool IsExternal { get; set; }
        public string DisplayName { get; set; }
    }
}