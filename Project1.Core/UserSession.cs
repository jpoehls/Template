using System;

namespace Project1.Core
{
    public static class UserSession
    {
        public static Action<string> Notify { get; set; }
    }
}