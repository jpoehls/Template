using System;

namespace Project1.Web.Helpers
{
    public static class Extensions
    {
        public static U Maybe<T, U>(this T t, Func<T, U> f) where T : class
        {
            return (t == null) ? default(U) : f(t);
        }
    }
}