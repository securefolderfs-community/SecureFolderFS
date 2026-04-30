using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SecureFolderFS.Shared.Logging
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that writes log messages to the IDE Debug Output window via <see cref="System.Diagnostics.Debug"/>.
    /// </summary>
    public sealed class DebugOutputLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, DebugOutputLogger> _loggers = new();
        private readonly LogLevel _minLevel;

        public DebugOutputLoggerProvider(LogLevel minLevel = LogLevel.Trace)
        {
            _minLevel = minLevel;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new DebugOutputLogger(name, _minLevel));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
