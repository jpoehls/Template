using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using MvcMiniProfiler;
using MvcMiniProfiler.MVCHelpers;
using Microsoft.Web.Infrastructure;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
//using System.Data;
//using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
//using MvcMiniProfiler.Data.EntityFramework;
//using MvcMiniProfiler.Data.Linq2Sql;

[assembly: WebActivator.PreApplicationStartMethod(
	typeof(Project1.Web.App_Start.MiniProfilerPackage), "PreStart")]

[assembly: WebActivator.PostApplicationStartMethod(
	typeof(Project1.Web.App_Start.MiniProfilerPackage), "PostStart")]


namespace Project1.Web.App_Start 
{
    public static class MiniProfilerPackage
    {
        public static void PreStart()
        {
            MiniProfiler.Settings.SqlFormatter = new MvcMiniProfiler.SqlFormatters.SqlServerFormatter();

			// To profile a standard DbConnection: 
			// var profiled = new ProfiledDbConnection(cnn, MiniProfiler.Current);

            //Make sure the MiniProfiler handles BeginRequest and EndRequest
            DynamicModuleUtility.RegisterModule(typeof(MiniProfilerStartupModule));

            //Setup profiler for Controllers via a Global ActionFilter
            GlobalFilters.Filters.Add(new ProfilingActionFilter());
        }

        public static void PostStart()
        {
            // Intercept ViewEngines to profile all partial views and regular views.
            // If you prefer to insert your profiling blocks manually you can comment this out
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            foreach (var item in copy)
            {
                ViewEngines.Engines.Add(new ProfilingViewEngine(item));
            }
        }
    }

    public class MiniProfilerStartupModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                var request = ((HttpApplication)sender).Request;
                //TODO: By default only local requests are profiled, optionally you can set it up
                //  so authenticated users are always profiled
                if (request.IsLocal) { MiniProfiler.Start(); }
            };


            // TODO: You can control who sees the profiling information
            /*
            context.AuthenticateRequest += (sender, e) =>
            {
                if (!CurrentUserIsAllowedToSeeProfiler())
                {
                    MvcMiniProfiler.MiniProfiler.Stop(discardResults: true);
                }
            };
            */

            context.EndRequest += (sender, e) =>
            {
                MiniProfiler.Stop();
            };
        }

        public void Dispose() { }
    }
}

