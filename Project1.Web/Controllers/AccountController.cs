using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using NHibernate;
using NHibernate.Linq;
using Project1.Core;
using Project1.Core.Domain;
using Project1.Core.Infrastructure;
using Project1.Web.Models;

namespace Project1.Web.Controllers
{
    public class AccountController : Controller
    {
        readonly ISession _session;
        public AccountController(ISession session)
        {
            _session = session;
        }

        [HttpGet, Authorize]
        public ActionResult Index()
        {
            ViewData.Model = new AccountModel
                                 {
                                     Email = Core.Domain.User.Current.Email.ToString()
                                 };
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult Index(AccountModel model)
        {
            // TODO: validate email address, Email email = model.Email;
            if (ModelState.IsValid)
            {
                Core.Domain.User.Current.Email = model.Email;
                _session.Save(Core.Domain.User.Current);
                _session.Flush();

                MvcFlash.Core.Flash.Success("Account details updated");
                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // GET: /Account/LogOn

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = ValidateUser(model.Email, model.Password);
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.Id.ToString(), model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The email address or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private User ValidateUser(string email, string password)
        {
            var user = _session.Query<User>().FirstOrDefault(u => u.Email == email);
            if (user != null && user.Password.CheckPassword(password))
            {
                return user;
            }
            return null;
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            // TODO: validate email address, Email email = model.Email;
            if (ModelState.IsValid)
            {
                try
                {
                    var user = Core.Domain.User.CreateNewUser(_session, model.Email, model.Password);
                    _session.Flush();

                    FormsAuthentication.SetAuthCookie(user.Id.ToString(), false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                catch (SecurityException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (!Core.Domain.User.Current.Password.CheckPassword(model.OldPassword))
                {
                    ModelState.AddModelError("", "The current password is incorrect");
                    return View(model);
                }

                Core.Domain.User.Current.Password.SetPassword(model.NewPassword);
                _session.Save(Core.Domain.User.Current);
                _session.Flush();
                return RedirectToAction("ChangePasswordSuccess");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }



        #region Password reset

        [HttpGet]
        public ActionResult RequestPasswordReset()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RequestPasswordReset(RequestPasswordResetModel model)
        {
            var user = _session.Query<User>().SingleOrDefault(x => x.Email == model.EmailAddress);
            if (user == null)
            {
                // REVIEW: is this a bad idea for security?
                ModelState.AddModelError("EmailAddress", "unrecognised email address");
                return View();
            }

            var code = user.RequestPasswordResetCode();
            var link = Url.Action("ResetPassword", "Account", new { code });
            UserSession.Notify("Reset link: http://" + Request.Url.Authority + link);

            return RedirectToAction("PasswordResetSent", "Account");
        }

        [HttpGet]
        public ActionResult PasswordResetSent()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword(string code)
        {
            var user = _session.Query<User>().SingleOrDefault(x => x.PasswordResetCode == code);
            if (user == null)
            {
                return RedirectToAction("InvalidResetPasswordRequest", "Account");
            }

            if (user.PasswordResetCodeRequestedAt == null ||
                (DateTime.Now - user.PasswordResetCodeRequestedAt.Value).Days >= 3)
            {
                // REVIEW: add some explanation that the code expired?
                return RedirectToAction("InvalidResetPasswordRequest", "Account");
            }

            var password = user.ResetPassword();
            var link = Url.Action("ChangePassword", "Account");
            UserSession.Notify("New password: " + password +
                ", reset link: http://" + Request.Url.Authority + link);

            return RedirectToAction("NewPasswordSent");
        }

        [HttpGet]
        public ActionResult NewPasswordSent()
        {
            return View();
        }

        [HttpGet]
        public ActionResult InvalidResetPasswordRequest()
        {
            return View();
        }

        #endregion

    }
}
