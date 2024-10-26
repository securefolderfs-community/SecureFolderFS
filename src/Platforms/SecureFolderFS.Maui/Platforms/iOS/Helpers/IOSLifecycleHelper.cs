using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Maui.Platforms.iOS.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class IOSLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        protected override string AppDirectory { get; } = Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory;

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize settings
            var settingsFolderPath = Path.Combine(AppDirectory, SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            _ = ex;
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                    //.AddSingleton<IPrinterService, WindowsPrinterService>()
                    .AddSingleton<IApplicationService, IOSApplicationService>()
                    .AddSingleton<IVaultCredentialsService, IOSVaultCredentialsService>()
                    .AddSingleton<IVaultFileSystemService, IOSVaultFileSystemService>()
                    .AddSingleton<IStorageService, IOSStorageService>()
                    .AddSingleton<IFileExplorerService, IOSFileExplorerService>()
                    .AddSingleton<ITelemetryService, DebugTelemetryService>()
                    .AddSingleton<IIapService, DebugIapService>()
                    .AddSingleton<IUpdateService, DebugUpdateService>()
                    .AddSingleton<ILocalizationService, ResourceLocalizationService>()

                    .WithMauiServices(settingsFolder)
                ;
        }
    }
}
