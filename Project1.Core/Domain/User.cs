using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Security;
using Harden.ValidationAttributes;
using NHibernate;
using NHibernate.Linq;
using Noodles;
using Project1.Core.Helpers;
using Project1.Core.Infrastructure;

namespace Project1.Core.Domain
{
    public class User : IHasChildren, IHasName, IHasParent<object>
    {
        private readonly ISession _session;

        public User(ISession session)
            : this()
        {
            _session = session;
        }
        public User()
        {
        }

        public virtual int Id { get; set; }

        public virtual string Username { get; set; }

        [NotNull]
        public virtual string Email { get; set; }

        [NotNull]
        public virtual Password Password { get; set; }

        #region Password reset
        public virtual string PasswordResetCode { get; set; }
        public virtual DateTimeOffset? PasswordResetCodeRequestedAt { get; set; }

        public virtual string RequestPasswordResetCode()
        {
            this.PasswordResetCode = Guid.NewGuid().ToString();
            this.PasswordResetCodeRequestedAt = DateTime.UtcNow;
            _session.Save(this);
            _session.Flush();
            return this.PasswordResetCode;
        }

        public virtual string ResetPassword()
        {
            var password = PronounceablePasswordGenerator.Generate(8);
            this.Password = password;
            this.PasswordResetCode = null;
            this.PasswordResetCodeRequestedAt = null;
            _session.Save(this);
            _session.Flush();
            return password;
        }
        #endregion

        #region Admin flags

        public virtual bool IsAdmin { get; set; }
        public virtual bool IsContentAdmin { get; set; }

        public static User CreateNewUser(ISession session, string email, Password password)
        {
            email = email.ToLower();
            if (session.Query<User>().Any(x => x.Email == email))
            {
                throw new SecurityException("An account already exists for this email address");
            }

            var user = new User
                           {
                               Email = email,
                               Password = password
                           };

            session.Save(user);

            // SETUP HACK: first user to register is made Admin
            // NOTE: accounting for hilo - this might even be not enough
            if (user.Id < 50)
            {
                var isFirstUser = session.Query<User>().Any() == false;
                if (isFirstUser)
                {
                    user.IsAdmin = true;
                    user.IsContentAdmin = true;
                    session.Save(user);
                }
            }
            return user;
        }

        #endregion

        #region noodles

        public virtual object GetChild(string name)
        {
            if (name.ToLowerInvariant() == "users") return new GenericNoodleCollection<object>(Current, "users", s => _session.Query<User>().Single(u => u.Id == int.Parse(s)));
            return null;
        }

        string IHasName.Name
        {
            get { return Current.Id == this.Id ? "app" : this.Id.ToString(); }
        }

        public virtual object Parent
        {
            get
            {
                return Current.Id == this.Id ? null : Current.GetChild("users");
            }
        }

        [AdminOnly, Show]
        public virtual void Impersonate()
        {
            FormsAuthentication.SetAuthCookie(this.Id.ToString(), false);
            HttpContext.Current.Response.Redirect("/", true);
        }

        public virtual bool? AllowImpersonate()
        {
            if (Id.ToString() == HttpContext.Current.User.Identity.Name)
                return false;
            else return null;
        }


        [AdminOnly, Show]
        public virtual void GrantAdmin()
        {
            this.IsAdmin = true;
            _session.Save(this);
            _session.Flush();
        }

        public virtual bool? AllowGrantAdmin()
        {
            if (Id.ToString() == HttpContext.Current.User.Identity.Name)
                return false;
            else return null;
        }

        [AdminOnly, Show]
        public virtual void RevokeAdmin()
        {
            this.IsAdmin = false;
            _session.Save(this);
            _session.Flush();
        }

        public virtual bool? AllowRevokeAdmin()
        {
            if (Id.ToString() == HttpContext.Current.User.Identity.Name)
                return false;
            else return null;
        }


        [AdminOnly, Show]
        public virtual void GrantContentAdmin()
        {
            this.IsContentAdmin = true;
            _session.Save(this);
            _session.Flush();
        }
        
        [AdminOnly, Show]
        public virtual void RevokeContentAdmin()
        {
            this.IsContentAdmin = false;
            _session.Save(this);
            _session.Flush();
        }

        #endregion

        #region Current user
        public static Func<User> GetCurrent { private get; set; }
        public static Action<User> SetCurrent { private get; set; }
        public static User Current
        {
            get { return GetCurrent == null ? null : GetCurrent(); }
            set { SetCurrent(value); }
        }
        public static IDisposable Scope(User user)
        {
            var get = GetCurrent;
            return new Scope(() => GetCurrent = () => user, () => GetCurrent = get);
        }
        #endregion

        #region Search
        public class SearchParameters
        {
            public string Q { get; set; }
            public bool? IsAdmin { get; set; }
        }
        public static IEnumerable<User> Search(ISession session, SearchParameters @params)
        {
            var queryString = "from User where Email like :q";
            if (@params.IsAdmin.HasValue)
            {
                queryString += " AND IsAdmin = :isAdmin";
            }
            var query = session.CreateQuery(queryString)
                // HACK, meh
                .SetParameter("q", @params.Q ?? "%");
            if (@params.IsAdmin.HasValue)
            {
                query.SetParameter("isAdmin", @params.IsAdmin);
            }

            return query.Enumerable<User>().Take(50);
        }
        #endregion
    }
}