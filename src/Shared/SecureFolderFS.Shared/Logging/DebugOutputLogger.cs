using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SecureFolderFS.Shared.Logging
{
    internal sealed class DebugOutputLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly LogLevel _minLevel;

        public DebugOutputLogger(string categoryName, LogLevel minLevel)
        {
            _categoryName = categoryName;
            _minLevel = minLevel;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
#if !DEBUG
            return false;
#endif
            return logLevel >= _minLevel && logLevel != LogLevel.None;
        }

        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var level = GetLevelTag(logLevel);

            Debug.WriteLine($"[{timestamp}] {level} {_categoryName}: {message}");

            if (exception is not null)
                Debug.WriteLine($"[{timestamp}] {level} {_categoryName}: >>> {exception}");
        }

        private static string GetLevelTag(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace => "[TRC]",
            LogLevel.Debug => "[DBG]",
            LogLevel.Information => "[INF]",
            LogLevel.Warning => "[WRN]",
            LogLevel.Error => "[ERR]",
            LogLevel.Critical => "[CRT]",
            _ => "[???]"
        };
    }
}
