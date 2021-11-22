using System.Web.Mvc;
using BLL.SurveySystem.Interfaces;

namespace Web.SurveySystem.Controllers
{
    [RoutePrefix("directories")]
    [Authorize]
    public class DirectoryController : Controller
    {
        private readonly ILoggerService<DirectoryController> loggingService;
        public DirectoryController(ILoggerService<DirectoryController> loggingService)
        {
            this.loggingService = loggingService;
        }

        [HttpGet]
        [Authorize]
        [Route("directory")]
        public ActionResult Index()
        {
            if (this.User.IsInRole("user"))
            {
                loggingService.Error($"{User.Identity.Name}  go to -> ReferenceDirectory");
                return RedirectToAction("Forbidden", "Error");
            }
            return View();
        }
    }
}