using System.Web.Mvc;
using Web.SurveySystem.Filters;

namespace Web.SurveySystem
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new BrowserCheckFilter());
            filters.Add(new MobileCheckFilter());
        }
    }
}