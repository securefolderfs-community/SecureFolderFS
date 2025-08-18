using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation;
using Windows.Storage;
using SecureFolderFS.Shared.Extensions;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class MacOsLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        protected override string AppDirectory { get; } = Path.Combine(ApplicationData.Current.LocalFolder.Path, nameof(SecureFolderFS));

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(AppDirectory, UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            ExceptionHelpers.TryWriteToFile(AppDirectory, ex);
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
                .Override<ISystemService, MacOsSystemService>(AddService.AddSingleton)
                .Override<IIapService, DebugIapService>(AddService.AddSingleton)
                .Override<IUpdateService, DebugUpdateService>(AddService.AddSingleton)
                .Override<IApplicationService, MacOsApplicationService>(AddService.AddSingleton)
                .Override<ILocalizationService, ResourceLocalizationService>(AddService.AddSingleton)
                .Override<IVaultFileSystemService, MacOsVaultFileSystemService>(AddService.AddSingleton)
                .Override<IVaultCredentialsService, MacOsVaultCredentialsService>(AddService.AddSingleton)

                .WithUnoServices(settingsFolder)
                
                ; // Finish service initialization
        }
    }
}
