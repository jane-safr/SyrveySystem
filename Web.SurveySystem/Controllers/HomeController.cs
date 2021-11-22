using System.Web.Mvc;

namespace Web.SurveySystem.Controllers
{
    [RoutePrefix("home")]
    [Authorize]
    public class HomeController : Controller
    {
        [Route("index")]
        public ActionResult Index()
        {
            return View();
        }
    }
}