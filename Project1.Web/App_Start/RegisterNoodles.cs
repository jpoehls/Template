using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Noodles.Infrastructure;
using Project1.Web.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(RegisterNoodles), "Start")]

namespace Project1.Web.App_Start
{
    public class RegisterNoodles
    {
        public static void Start()
        {
            GlobalFilters.Filters.Add(new GlobalFixUserExceptionsAttribute());
            GlobalFilters.Filters.Add(new ModelStateTempDataTransferAttribute());

        }
    }

}