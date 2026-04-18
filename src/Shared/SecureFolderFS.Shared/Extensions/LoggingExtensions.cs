using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace SecureFolderFS.Shared.Extensions
{
    public static class LoggingExtensions
    {
        public static long GetPerformanceScope(this ILogger logger)
        {
            return Stopwatch.GetTimestamp();
        }

        public static void LogPerformance(this ILogger logger, long scope, int minThresholdMs = -1, [CallerMemberName] string caller = "")
        {
            var elapsed = Stopwatch.GetElapsedTime(scope);
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            if (minThresholdMs < 0 || elapsed.TotalMilliseconds > minThresholdMs)
                logger.LogDebug("{Caller} completed in {ElapsedMs:F2}ms", caller, elapsed.TotalMilliseconds);
        }
    }
}
