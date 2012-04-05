using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Project1.Core.Helpers;
using Project1.Web.Helpers;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(Project1.Web.App_Start.SystemTimePackage), "PreStart")]

namespace Project1.Web.App_Start
{
    public static class SystemTimePackage
    {
        public static void PreStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(SystemTimeStartupModule));
        }
    }

    public class SystemTimeStartupModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                var request = ((HttpApplication)sender).Request;
                if (request.IsLocal) // don't allow this on live
                {
                    // TODO: this is not threadsafe, but only happens on test so meh.
                    var dateTimeOffset = request.Cookies.GetSystemTimeOverride();
                    if (dateTimeOffset.HasValue)
                    {
                        SystemTime.GetNow = () => { return dateTimeOffset.Value; };
                    }
                    else
                    {
                        SystemTime.GetNow = SystemTime.GetDateTimeOffsetUtcNow;
                    }
                }
            };
        }

        public void Dispose() { }
    }

}