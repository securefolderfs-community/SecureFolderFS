using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Shared.Logging
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that writes log messages to a file on disk.
    /// </summary>
    public sealed class FileOutputLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, FileOutputLogger> _loggers = new();
        private readonly object _writeLock = new();
        private readonly string _filePath;
        private readonly LogLevel _minLevel;

        public FileOutputLoggerProvider(string filePath, LogLevel minLevel = LogLevel.Information)
        {
            _filePath = filePath;
            _minLevel = minLevel;

            var directory = Path.GetDirectoryName(filePath);
            if (directory is not null)
                Directory.CreateDirectory(directory);
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileOutputLogger(name, _minLevel, this));
        }

        internal void WriteMessage(string message)
        {
            lock (_writeLock)
                SafetyHelpers.NoFailure(() => File.AppendAllText(_filePath, message + Environment.NewLine));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
