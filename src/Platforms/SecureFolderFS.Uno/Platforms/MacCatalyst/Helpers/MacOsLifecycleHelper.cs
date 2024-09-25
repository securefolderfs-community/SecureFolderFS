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

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class MacOsLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, nameof(SecureFolderFS), UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
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
                //.AddSingleton<IPrinterService, WindowsPrinterService>()
                .AddSingleton<IVaultService, MacOsVaultService>()
                .AddSingleton<IApplicationService, MacOsApplicationService>()
                .AddSingleton<IVaultManagerService, MacOsVaultManagerService>()
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()
                
                .WithUnoServices(settingsFolder)
                
                ;
        }
    }
}
