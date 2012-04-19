using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Harden;
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
            
            Noodles.NodeMethodsRuleRegistry.ShowMethodRules.Insert(0,
                (target, info) => Allow.Call(target, info) ? null as bool? : false);
            Noodles.NoodleResultBuilderExtension.AddExceptionHandler<ValidationException>((ex, cc) =>
            {
                foreach (var error in ex.Errors)
                {
                    cc.Controller.ViewData.ModelState.AddModelError(error.Field, error.Message);
                }
            });
        }
    }

}