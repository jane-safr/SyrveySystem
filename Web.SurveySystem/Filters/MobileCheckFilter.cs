using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace Web.SurveySystem.Filters
{
    public class MobileCheckFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Debug.WriteLine("Desktop");
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (!string.IsNullOrEmpty(filterContext.HttpContext.Request.UserAgent) &&
                !filterContext.HttpContext.Request.UserAgent.ToLower().Contains("ipad") &&
                filterContext.HttpContext.Request.Browser != null &&
                filterContext.HttpContext.Request.Browser.IsMobileDevice)
            {
                filterContext.Result = new ContentResult
                {
                    Content = String.IsInterned(
                        "<div>Доступ с мобильных устройств запрещен / Access from mobile devices is prohibited</div>")
                };
                filterContext.HttpContext.Response.Write(string.Format(
                    "<div> Доступ с мобильных устройств запрещен / Access from mobile devices is prohibited </div>"));
                filterContext.HttpContext.Response.End();
            }
        }
    }
}