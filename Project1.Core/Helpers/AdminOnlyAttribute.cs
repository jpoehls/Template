using System;
using System.Linq;
using System.Reflection;
using Project1.Core.Domain;

namespace Project1.Core.Helpers
{
    class AdminOnlyAttribute : Attribute
    {
        static AdminOnlyAttribute()
        {
            Harden.Allow.Rules.Add(AdminOnlyAttribute.AllowRule);
        }

        private static bool? AllowRule(object obj, MethodInfo methodBeingcalled)
        {
            if (methodBeingcalled.GetCustomAttributes(typeof(AdminOnlyAttribute), false).Any())
            {
                if (!User.Current.IsAdmin) return false;
            }
            return null;
        }
    }
}