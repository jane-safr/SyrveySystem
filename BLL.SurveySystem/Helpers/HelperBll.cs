using System;
using System.Text;
using System.Text.RegularExpressions;

namespace BLL.SurveySystem.Helpers
{
    public class HelperBll
    {
        public static bool IsGuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            Guid newGuid;
            return Guid.TryParse(value, out newGuid);
        }
        public static bool ValidateMail(string email)
        {
            string expr = "^[_A-Za-z0-9-]+(\\.[_A-Za-z0-9-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9-]+)*(\\.[A-Za-z]{2,})$";
            return Regex.Match(email, expr, RegexOptions.IgnoreCase).Success;
        }
        public static string Base64StringDecode(string encodedString)
        {
            var bytes = Convert.FromBase64String(encodedString);
            var decodedString = Encoding.UTF8.GetString(bytes);
            return decodedString;
        }
        public static string Base64StringEncode(string originalString)
        {
            var bytes = Encoding.UTF8.GetBytes(originalString);
            var encodedString = Convert.ToBase64String(bytes);
            return encodedString;
        }

        public static bool ValidateText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            return Regex.Match(text, "^[_A-Za-z0-9-А-Яа-яЁёğüşöçİĞÜŞÖÇıI+Ø .,!?*;«»'&\"\r\n{}()№;@|/-:$%^~Ёё–]{2,3500}$", RegexOptions.IgnoreCase).Success;
        }
        public static bool ValidateUrlNotificationType(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            string expr = @"^(#=BaseUrl#)" + @"[a-z0-1/\?=#]{0,100}$";
            return Regex.Match(text, expr, RegexOptions.IgnoreCase).Success;
        }
        public static string DeleteRowTabToText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return Regex.Replace(text, @"\r\n?|\n|\t", string.Empty);
        }


        //public static string GenerateShortLink(int code, string baseUrl)
        //{
        //    if (code < 1)
        //        return string.Empty;
        //    if(string.IsNullOrEmpty(baseUrl))
        //        return string.Empty;
        //    return WebEncoders.Base64UrlEncode(BitConverter.GetBytes(Id));
            
        //}


        public static bool CheckUrlValid(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }
            var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult);
            if (result)
            {
                uriResult = new Uri(url);
                if (uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp)
                    return true;
            }
            return false;
        }
    }
}
