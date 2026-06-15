using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation;
using SecureFolderFS.Uno.ServiceImplementation;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;
#if APP_PLATFORM_PRESENT
using SecureFolderFS.Sdk.AppPlatform.Helpers;
using SecureFolderFS.Sdk.AppPlatform.Services;
using SecureFolderFS.Shared.ComponentModel;
#endif

namespace SecureFolderFS.Uno.Platforms.Desktop.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class SkiaLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override string AppDirectory { get; } = Directory.GetCurrentDirectory();

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(AppDirectory, Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            var formattedException = ExceptionHelpers.FormatException(ex);
            if (formattedException is null)
                return;

            ExceptionHelpers.WriteSessionFile(AppDirectory, formattedException);
            ExceptionHelpers.WriteAggregateFile(AppDirectory, formattedException);
        }

        /// <inheritdoc/>
        public override void LogException(Exception? ex)
        {
            if (ex?.InnerException?.Message.Contains("The remote party") ?? false)
                return;

            base.LogException(ex);
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                    .Override<IIapService, DebugIapService>(AddService.AddSingleton)
                    .Override<ISystemService, SkiaSystemService>(AddService.AddSingleton)
                    .Override<IPrivacyService, SkiaPrivacyService>(AddService.AddSingleton)
                    .Override<IUpdateService, DebugUpdateService>(AddService.AddSingleton)
                    .Override<ITelemetryService, DebugTelemetryService>(AddService.AddSingleton)
                    .Override<IApplicationService, SkiaApplicationService>(AddService.AddSingleton)
                    .Override<ILocalizationService, ResourceLocalizationService>(AddService.AddSingleton)
                    .Override<IVaultFileSystemService, SkiaVaultFileSystemService>(AddService.AddSingleton)
                    .Override<IVaultCredentialsService, SkiaVaultCredentialsService>(AddService.AddSingleton)
#if APP_PLATFORM_PRESENT
                    .Override<IOidcProvider, BrowserAuthProvider>(AddService.AddSingleton)
#endif
#if APP_PLATFORM_PRESENT && !WINDOWS
                    .AddSingleton<IDeviceKeyStore>(new SkiaDeviceKeyStore(settingsFolder.Id))
                    .AddSingleton<IAccountProvider>(sp => new AppPlatformAccountProvider(sp.GetRequiredService<IDeviceKeyStore>()))
#endif

                    .WithUnoServices(settingsFolder)
                ;
        }
    }
}
