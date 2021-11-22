using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using BLL.SurveySystem.Interfaces;
using Web.SurveySystem.Helpers;
using Web.SurveySystem.Models.ViewModels;

namespace Web.SurveySystem.Controllers
{
    [RoutePrefix("roles")]
    public class RoleController : Controller
    {
        private readonly IRoleService roleService;
        private readonly ILoggerService<RoleController> loggingService;

        public RoleController(ILoggerService<RoleController> logServ, IRoleService roleServ)
        {
            this.loggingService = logServ;
            this.roleService = roleServ;
        }

        [HttpGet]
        [Authorize]
        [Route("index")]
        public ActionResult Index()
        {
            if (!User.IsInRole("admin") && !User.IsInRole("manager"))
            {
                loggingService.Error("Access denied");
                return new HttpStatusCodeResult(403);
            }

            return View();
        }

        [HttpGet]
        [Authorize]
        [OutputCache(Duration = 30, Location = OutputCacheLocation.Downstream)]
        [Route("all")]
        public async Task<JsonNetResult> GetRoles()
        {
            try
            {
                var roles = (await roleService.GetRolesAsync()).Select(r => new RoleVM
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                }).ToList();
                return new JsonNetResult(new {success = true, data = roles});
            }
            catch (Exception e)
            {
                loggingService.Error($"GetRole: {e.Message}");
                return JsonNetResult.Failure(e.Message);
            }
        }
    }
}