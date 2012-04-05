using System;

namespace Project1.Core.Helpers
{
    public static class SystemTime
    {
        public static readonly Func<DateTimeOffset> GetDateTimeOffsetUtcNow =
            () => { return DateTimeOffset.UtcNow; };
        public static Func<DateTimeOffset> GetNow = GetDateTimeOffsetUtcNow;
        public static DateTimeOffset Now { get { return GetNow(); } }
    }
}