using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.ServiceImplementation;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.UI.Helpers
{
    public abstract class BaseLifecycleHelper : IAsyncInitialize
    {
        public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

        protected abstract string AppDirectory { get; }

        /// <inheritdoc/>
        public virtual Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            return Task.CompletedTask;
        }

        public virtual void LogException(Exception? ex)
        {
            var formattedException = ExceptionHelpers.FormatException(ex);
            Debug.WriteLine(formattedException);

            // Please check the Application Output for exception details
            // On Microsoft Visual Studio, go to View -> Output Window
            // On JetBrains Rider, go to View -> Tool Windows -> Debug -> Debug Output
            Debugger.Break();

#if !DEBUG
            if (ex is not null && DI.OptionalService<ITelemetryService>() is { } telemetryService)
                telemetryService.TrackException(ex);

            LogExceptionToFile(ex);
#else
            if (!Debugger.IsAttached)
                LogExceptionToFile(ex);
#endif
        }

        protected virtual IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return ServiceCollection

                // Singleton services
                    .Foundation<IVaultService, VaultService>(AddService.AddSingleton)
                    .Foundation<IVaultService, VaultService>(AddService.AddSingleton)
                    .Foundation<IIapService, DebugIapService>(AddService.AddSingleton)
                    .Foundation<IRecycleBinService, RecycleBinService>(AddService.AddSingleton)
                    .Foundation<IVaultHealthService, VaultHealthService>(AddService.AddSingleton)
                    .Foundation<IVaultManagerService, VaultManagerService>(AddService.AddSingleton)
                    .Foundation<IChangelogService, GitHubChangelogService>(AddService.AddSingleton)
                    .Foundation<IVaultPersistenceService, VaultPersistenceService>(AddService.AddSingleton, _ => new VaultPersistenceService(settingsFolder))
                
#if DEBUG
                    .Foundation<ITelemetryService, DebugTelemetryService>(AddService.AddSingleton)
#else
                    .Foundation<ITelemetryService, SentryTelemetryService>(AddService.AddSingleton)
#endif

                ; // Finish service initialization
        }

        public abstract void LogExceptionToFile(Exception? ex);
    }
}
