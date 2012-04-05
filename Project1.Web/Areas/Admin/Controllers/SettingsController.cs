using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Project1.Web.Helpers;

namespace Project1.Web.Areas.Admin.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet]
        public ActionResult SystemTime()
        {
            ViewData.Model = Request.Cookies.GetSystemTimeOverride();
            return View();
        }

        [HttpPost]
        public ActionResult SystemTime(DateTimeOffset systemTime, string update, string clear)
        {
            if (update != null)
            {
                Response.Cookies.SetSystemTimeOverride(systemTime);
            }
            if (clear != null)
            {
                Response.Cookies.ClearSystemTimeOverride();
            }
            return RedirectToAction("SystemTime");
        }

    }
}
