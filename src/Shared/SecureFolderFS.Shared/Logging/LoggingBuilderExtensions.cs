using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SecureFolderFS.Shared.Logging
{
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="DebugOutputLoggerProvider"/> that writes to the IDE Debug Output window.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
        /// <param name="minLevel">The minimum log level to output.</param>
        /// <returns>The <see cref="ILoggingBuilder"/> so that calls can be chained.</returns>
        public static ILoggingBuilder AddDebugOutput(this ILoggingBuilder builder, LogLevel minLevel = LogLevel.Trace)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new DebugOutputLoggerProvider(minLevel));
            return builder;
        }

        /// <summary>
        /// Adds <see cref="FileOutputLoggerProvider"/> that writes to a log file on disk.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to configure.</param>
        /// <param name="filePath">The absolute path to the log file.</param>
        /// <param name="minLevel">The minimum log level to output.</param>
        /// <returns>The <see cref="ILoggingBuilder"/> so that calls can be chained.</returns>
        public static ILoggingBuilder AddFileOutput(this ILoggingBuilder builder, string filePath, LogLevel minLevel = LogLevel.Information)
        {
            builder.Services.AddSingleton<ILoggerProvider>(new FileOutputLoggerProvider(filePath, minLevel));
            return builder;
        }
    }
}
