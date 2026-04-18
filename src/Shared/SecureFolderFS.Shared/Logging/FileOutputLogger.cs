using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SecureFolderFS.Shared.Logging
{
    internal sealed class FileOutputLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly LogLevel _minLevel;
        private readonly FileOutputLoggerProvider _provider;

        public FileOutputLogger(string categoryName, LogLevel minLevel, FileOutputLoggerProvider provider)
        {
            _categoryName = categoryName;
            _minLevel = minLevel;
            _provider = provider;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
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
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var level = GetLevelTag(logLevel);

            var line = $"[{timestamp}] {level} {_categoryName}: {message}";
            if (exception is not null)
                line += $"{Environment.NewLine}    >>> {exception}";

            _provider.WriteMessage(line);
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
