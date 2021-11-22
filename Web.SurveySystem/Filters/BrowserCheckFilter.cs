using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace Web.SurveySystem.Filters
{
    public class BrowserCheckFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Debug.WriteLine("Browser valid");
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.HttpContext.Request.Browser != null &&
                (filterContext.HttpContext.Request.Browser.Browser.IndexOf("InternetExplore", StringComparison.Ordinal) > -1
                 || filterContext.HttpContext.Request.UserAgent?.IndexOf("Edge", StringComparison.Ordinal) > -1))
            {
                filterContext.Result = new ContentResult { Content = "<div>Ваш браузер не поддерживается системой / Your browser is not supported by the system</div>" };
                filterContext.HttpContext.Response.Write(string.Format("<div>⛔️ Cистема не поддерживается в браузере Internet Explorer, Edge. Для корректной работы используйте Google Chrome, Firefox, Opera. / This system is not supported by browser Internet Explorer, Edge. For correct work please use Google Chrome, Firefox, Opera. </div>"));
                filterContext.HttpContext.Response.End();
            }
        }
    }
}