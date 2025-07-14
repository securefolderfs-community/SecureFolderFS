using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

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
                    //.Override<IIapService, IOSIapService>(AddService.AddSingleton)
                    .Override<IIapService, DebugIapService>(AddService.AddSingleton)
                    .Override<IMediaService, IOSMediaService>(AddService.AddSingleton)
                    .Override<ISystemService, IOSSystemService>(AddService.AddSingleton)
                    .Override<IStorageService, IOSStorageService>(AddService.AddSingleton)
                    .Override<IUpdateService, DebugUpdateService>(AddService.AddSingleton)
                    .Override<IApplicationService, IOSApplicationService>(AddService.AddSingleton)
                    .Override<IFileExplorerService, IOSFileExplorerService>(AddService.AddSingleton)
                    .Override<ILocalizationService, ResourceLocalizationService>(AddService.AddSingleton)
                    .Override<IVaultFileSystemService, IOSVaultFileSystemService>(AddService.AddSingleton)
                    .Override<IVaultCredentialsService, IOSVaultCredentialsService>(AddService.AddSingleton)

                    .WithMauiServices(settingsFolder)
                ;
        }
    }
}
