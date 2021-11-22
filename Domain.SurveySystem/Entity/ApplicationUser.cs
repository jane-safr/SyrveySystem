using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Domain.SurveySystem.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsExternal { get; set; }
        public string DisplayName { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            userIdentity.AddClaim(new Claim("DisplayName", this.DisplayName));
            return userIdentity;
        }
    }
}