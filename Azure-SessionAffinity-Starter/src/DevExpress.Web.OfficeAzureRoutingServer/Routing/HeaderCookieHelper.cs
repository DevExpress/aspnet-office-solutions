using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace DevExpress.Web.OfficeAzureRoutingServer {

    class HeaderCookieHelper {
        const string CookieHeaderName = "Cookie";

        public static void PatchHeaderCookieGUID(HttpApplication application, string cookieNameToPatch, string newCookieValue) {
            string cookies = GetRequestHeaderValue(application, CookieHeaderName);
            string patchedCookies = PatchCookies(cookies, cookieNameToPatch, newCookieValue);
            SetRequestHeaderValue(application, CookieHeaderName, patchedCookies);
        }
        public static bool HasServerAffinity(HttpApplication application, string cookieName) {
            string cookies = GetRequestHeaderValue(application, CookieHeaderName);
            return CheckCookieExists(cookieName, cookies);
        }
        public static string GetCurrentAffinityValue(HttpApplication application, string cookieName) {
            string cookies = GetRequestHeaderValue(application, CookieHeaderName);
            Regex regex = new Regex(cookieName + "=([0-9A-Fa-f]{64})");
            Match m = regex.Match(cookies);
            if(m.Success)
                return m.Groups[1].Value;
            return string.Empty;
        }

        static string GetRequestHeaderValue(HttpApplication application, string headerName) {
            if(application.Request.Headers.AllKeys.Contains(headerName)) {
                return application.Request.Headers[headerName];
            }
            return string.Empty;
        }

        static void SetRequestHeaderValue(HttpApplication application, string headerName, string headerValue) {
            NameValueCollection headers = application.Request.Headers;
            headers.GetType().GetProperty("IsReadOnly", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).SetValue(headers, false, null);
            if(headers.AllKeys.Contains<string>(headerName)) {
                headers[headerName] = headerValue;
            } else {
                headers.Add(headerName, headerValue);
            }
        }

        static string PatchCookies(string cookies, string cookieName, string cookieValue) {
            cookies = CleanUpEmptyCookieValue(cookies, cookieName);
            if(CheckCookieExists(cookieName, cookies)) 
                cookies = ReplaceCookieGUID(cookies, cookieName, cookieValue);
             else 
                cookies = AppendCookieGUID(cookies, cookieName, cookieValue);
            
            return cookies;
        }

        static string CleanUpEmptyCookieValue(string cookies, string cookieName) {
            Regex regex = new Regex(cookieName + "=(;|$)");
            return regex.Replace(cookies, string.Empty);
        }

        static bool CheckCookieExists(string cookieName, string cookieValue) {
            return cookieValue.Contains(cookieName + "=");
        }

        static string AppendCookieGUID(string cookies, string cookieName, string cookieValue) {
            if(!string.IsNullOrEmpty(cookies) && !cookies.TrimEnd().EndsWith(";"))
                cookies += ";";
            cookies += cookieName + "=" + cookieValue;
            return cookies;
        }

        static string ReplaceCookieGUID(string cookies, string cookieName, string cookieValue) {
            Regex regex = new Regex(cookieName + "=([0-9A-Fa-f]{64}|$)");
            string replacement = cookieName + "=" + cookieValue;
            if(regex.IsMatch(cookies))
                return regex.Replace(cookies, replacement);
            regex = new Regex(cookieName + "=;");
            if(regex.IsMatch(cookies))
                return cookies.Replace(cookieName + "=", replacement);
            return cookies;
        }

    }
}
