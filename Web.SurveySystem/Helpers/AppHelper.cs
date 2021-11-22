using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;

namespace Web.SurveySystem.Helpers
{
    public static class AppHelper
    {
        public static HtmlString ApplicationVersion(this HtmlHelper helper)
        {
            var asm = Assembly.GetExecutingAssembly();
            var version = asm.GetName().Version;
            if (version != null &&
                asm.GetCustomAttributes(typeof(AssemblyProductAttribute), true).FirstOrDefault() is
                    AssemblyProductAttribute product)
            {
                return new HtmlString($"<span style='font-weight: normal;' title='Текущая версия / Current version'>v.{version.Major}.{version.Minor}.{version.Build}</span>");
            }

            return new HtmlString(string.Empty);
        }

        public static string GetFullName(this IPrincipal usr)
        {
            var fullNameClaim = ((ClaimsIdentity) usr.Identity).FindFirst("DisplayName");
            if (fullNameClaim != null)
                return fullNameClaim.Value;
            return ""; // если DisplayName не найден. Claim Добавлен в Login: identity.AddClaim(new Claim("DisplayName", userAd.DisplayName));
        }
    }
}