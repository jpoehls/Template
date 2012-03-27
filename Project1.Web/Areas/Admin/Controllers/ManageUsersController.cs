using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;
using NHibernate.Linq;
using Project1.Core.Domain;
using Project1.Web.Areas.Admin.Models;

namespace Project1.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersController : Controller
    {
        private readonly ISession _session;

        public ManageUsersController(ISession session)
        {
            _session = session;
        }

        //
        // GET: /Admin/ManageAdmins/

        public ActionResult Index(User.SearchParameters @params)
        {
            var model = new ManageUsersModel {SearchParameters = @params};
            if (@params.IsAdmin != null || @params.Q != null)
            {
                model.Users = Core.Domain.User.Search(_session, @params);
            }
            ViewData.Model = model;
            return View();
        }
    }
}
