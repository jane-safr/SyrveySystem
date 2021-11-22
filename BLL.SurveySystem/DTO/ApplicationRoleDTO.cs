using Microsoft.AspNet.Identity.EntityFramework;

namespace BLL.SurveySystem.DTO
{
    public class ApplicationRoleDTO : IdentityRole
    {
        public string Description { get; set; }
        public bool IsShown { get; set; }
    }
}