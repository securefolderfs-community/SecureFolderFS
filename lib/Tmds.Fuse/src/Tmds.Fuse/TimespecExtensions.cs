using System;
using Tmds.Linux;
using static Tmds.Linux.LibC;

namespace Tmds.Fuse
{
    public static class TimespecExtensions
    {
        public static DateTime ToDateTime(this timespec ts)
        {
            if (ts.IsNow() || ts.IsOmit())
            {
                throw new InvalidOperationException("Cannot convert meta value to DateTime");
            }
            return new DateTime(UnixEpochTicks + TimeSpan.TicksPerSecond * ts.tv_sec + ts.tv_nsec / 100, DateTimeKind.Utc);
        }

        public static timespec ToTimespec(this DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            long ticks = dateTime.Ticks - UnixEpochTicks;
            long sec = ticks / TimeSpan.TicksPerSecond;
            ticks -= TimeSpan.TicksPerSecond * sec;
            long nsec = ticks * 100;
            return new timespec { tv_sec = sec, tv_nsec = (int)nsec };
        }

        public static bool IsNow(this timespec ts)
            => ts.tv_nsec == UTIME_NOW;

        public static bool IsOmit(this timespec ts)
            => ts.tv_nsec == UTIME_OMIT;

        private const long UnixEpochTicks = 621355968000000000;
    }
}