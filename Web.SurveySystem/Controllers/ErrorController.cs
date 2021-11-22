using System.Web.Mvc;

namespace Web.SurveySystem.Controllers
{
   
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }
        [Authorize]
        public ActionResult TestFinished()
        {
            Response.StatusCode = 409;
            return View();
        }
        [Authorize]
        public ActionResult TestOverdue()
        {
            Response.StatusCode = 409;
            return View();
        }
    }
}