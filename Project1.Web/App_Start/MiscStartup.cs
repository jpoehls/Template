using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Project1.Core;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(Project1.Web.App_Start.MiscStartup), "PreStart")]

namespace Project1.Web.App_Start
{
    public class MiscStartup
    {
        public static void PreStart()
        {
            if (Environment.MachineName == "LIVE_BUILD_SERVER")
            {
                // TODO: UserSession.Notify = s => EmailServer.(...);
            }
            else
            {
                UserSession.Notify = s => MvcFlash.Core.Flash.Notice(s);
            }
        }
    }
}