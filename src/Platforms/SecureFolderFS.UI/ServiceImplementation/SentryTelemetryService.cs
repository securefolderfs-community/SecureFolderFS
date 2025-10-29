using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Api;
using Sentry;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ITelemetryService"/>
    public sealed class SentryTelemetryService : ITelemetryService
    {
        private readonly IApplicationService _applicationService;
        private IDisposable? _sentryCompletion;

        public SentryTelemetryService(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <inheritdoc/>
        public Task<bool> IsEnabledAsync()
        {
            return Task.FromResult(SentrySdk.IsEnabled);
        }

        /// <inheritdoc/>
        public Task EnableTelemetryAsync()
        {
            if (string.IsNullOrEmpty(ApiKeys.SentryDsnKey))
                return Task.CompletedTask;

            _sentryCompletion = SentrySdk.Init(options =>
            {
                options.Dsn = ApiKeys.SentryDsnKey;
                options.AutoSessionTracking = true;

                options.Release = _applicationService.AppVersion.ToString();
                options.TracesSampleRate = 0.80;
                options.ProfilesSampleRate = 0.40;

#if DEBUG
                options.Debug = true;
                options.Environment = "Staging";
#else
                options.Environment = "Production";
#endif
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DisableTelemetryAsync()
        {
            _sentryCompletion?.Dispose();
            _sentryCompletion = null;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void TrackMessage(string eventName, Severity severity)
        {
            SentrySdk.CaptureMessage(eventName, severity switch
            {
                Severity.Warning => SentryLevel.Warning,
                Severity.Critical => SentryLevel.Error,
                _ => SentryLevel.Info
            });
        }

        /// <inheritdoc/>
        public void TrackException(Exception exception)
        {
            SentrySdk.CaptureException(exception);
        }
    }
}
