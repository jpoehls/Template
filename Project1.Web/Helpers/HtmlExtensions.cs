using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Project1.Web.Helpers
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString AdminActionLink(this HtmlHelper<dynamic> htmlHelper, string linkText, string controllerName)
        {
            return htmlHelper.ActionLink(linkText, "Index", controllerName, new { Area = "Admin"}, null);
        }
    }
}