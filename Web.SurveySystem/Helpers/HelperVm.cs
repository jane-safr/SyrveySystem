using System;
using System.Text.RegularExpressions;

namespace Web.SurveySystem.Helpers
{
    public class HelperVm
    {
        public static string RemoveHtmlCode(string text)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text) && text.StartsWith("<table width='100%'"))
                {
                    return "Текст сообщения скрыт / Message text is hidden";
                }
                var resText = !string.IsNullOrWhiteSpace(text)
                    ? text.Replace("<strong>", string.Empty).Replace("</strong>", string.Empty)
                        .Replace("<i>", string.Empty).Replace("</i>", string.Empty)
                        .Replace("<br/>", string.Empty).Replace("<hr/>", string.Empty)
                        .Replace("<em>", string.Empty).Replace("</em>", string.Empty)
                        .Replace("<span style='font-weight:bold; font-size: 25px;'>", string.Empty).Replace("</span>", string.Empty)
                        .Replace("<span style=\"font-weight:bold; font-size: 25px;\">", string.Empty)
                        .Replace("<h3>", string.Empty).Replace("</h3>", string.Empty)
                        .Replace("<p>", string.Empty).Replace("</p>", string.Empty)
                        .Replace("<br />", string.Empty).Replace("&nbsp;", string.Empty)
                    : string.Empty;
                return resText;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static bool ValidateMail(string email)
        {
            string expr = "^[_A-Za-z0-9-]+(\\.[_A-Za-z0-9-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*(\\.[A-Za-z]{2,})$";
            return Regex.Match(email, expr, RegexOptions.IgnoreCase).Success;
        }
        public static bool IsGuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            Guid newGuid;
            return Guid.TryParse(value, out newGuid);
        }
    }
}