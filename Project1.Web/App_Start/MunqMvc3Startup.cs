using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Harden;
using Munq;
using Munq.LifetimeManagers;
using Munq.MVC3;
using NHibernate;
using NHibernate.Linq;
using Project1.Core;
using Project1.Core.Domain;
using Project1.Core.Infrastructure;
using Project1.Web.Controllers;
using Project1.Web.Infrastructure;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(Project1.Web.App_Start.MunqMvc3Startup), "PreStart")]

namespace Project1.Web.App_Start
{
    public static class MunqMvc3Startup
    {
        public static void PreStart()
        {
            DependencyResolver.SetResolver(new MunqDependencyResolver());

            var container = MunqDependencyResolver.Container;

            ModelBinderProviders.BinderProviders.Add(new ImplicitOperatorBinderProvider());

            Fac.Default.Interceptors.Add(x => new HardenInterceptor());
            Fac.Default.SetServiceLocator(container.Resolve, container.CanResolve);

            var requestLifetime = new RequestLifetime();

            container.Register<HttpContextBase>(x => new HttpContextWrapper(HttpContext.Current)).WithLifetimeManager(requestLifetime);
            NHibernateConfigurator.Configure(container, requestLifetime);

            // TODO: FilterProviders.Providers.Add(new AntiForgeryTokenFilterProvider());
            GlobalFilters.Filters.Add(new HandleErrorAttribute());
            
            Logger.LogException = ex => Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            Infrastructure.RoleProvider.GetUser = username => container.Resolve<ISession>().Load<User>(int.Parse(username));

            User.GetCurrent = () =>
            {
                var ctx = HttpContext.Current;

                var user = ctx.Items["User"] as User;
                if (user != null) return user;
                if (!ctx.Request.IsAuthenticated)
                {
                    return null;
                }

                ctx.Items["User"] = (user = FetchCurrentUser(ctx, container));
                if (user == null)
                {
                    FormsAuthentication.SignOut();
                }

                return user;
            };
            User.SetCurrent = u => { HttpContext.Current.Items["User"] = u; };
        }

        private static User FetchCurrentUser(HttpContext ctx, IocContainer container)
        {
            return container.Resolve<ISession>().Query<User>().Single(u => u.Id == int.Parse(ctx.User.Identity.Name));
        }
    }
}


