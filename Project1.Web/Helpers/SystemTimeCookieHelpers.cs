using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project1.Web.Helpers
{
    public static class SystemTimeCookieHelpers
    {
        public static DateTimeOffset? AsDateTimeOffset(this string value)
        {
            DateTimeOffset d;
            return DateTimeOffset.TryParse(value, out d) ? (DateTimeOffset?)d : null;
        }

        private const string SystemTimeOverrideCookieName = "SystemTimeOverride";
        public static DateTimeOffset? GetSystemTimeOverride(this HttpCookieCollection cookies)
        {
            return cookies[SystemTimeOverrideCookieName].Maybe(x => x.Value).AsDateTimeOffset();
        }

        public static void SetSystemTimeOverride(this HttpCookieCollection cookies, DateTimeOffset offset)
        {
            cookies.Remove(SystemTimeOverrideCookieName);
            var cookie = new HttpCookie(SystemTimeOverrideCookieName);
            cookie.Value = offset.ToString();
            cookies.Add(cookie);
        }

        public static void ClearSystemTimeOverride(this HttpCookieCollection cookies)
        {
            cookies.Remove(SystemTimeOverrideCookieName);
            cookies.Add(new HttpCookie(SystemTimeOverrideCookieName) { Expires = DateTime.Now.AddDays(-1) });
        }
    }
}