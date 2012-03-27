using System;

namespace Project1.Core
{
    public static class Logger
    {
        static Logger()
        {
            LogException = ex => { };
        }
        public static Action<Exception> LogException { get; set; }
    }
}
