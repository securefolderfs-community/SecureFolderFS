using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.Extensions;
using SecureFolderFS.Uno.Skia.Gtk.ServiceImplementation;
using Windows.Storage;

namespace SecureFolderFS.Uno.Skia.Gtk.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class SkiaLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            var settingsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
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
                .AddSingleton<IVaultService, SkiaVaultService>()
                .AddSingleton<IApplicationService, SkiaApplicationService>()
                .AddSingleton<IVaultManagerService, SkiaVaultManagerService>()
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()
                
                .WithUnoServices(settingsFolder)
                
                ;
        }
    }
}
