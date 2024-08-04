using OwlCore.Storage;
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
        public override void LogExceptionToFile(Exception? ex)
        {
            _ = ex;
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                    //.AddSingleton<IPrinterService, WindowsPrinterService>()
                    //.AddSingleton<IApplicationService, SkiaApplicationService>()
                    .AddSingleton<IVaultService, IOSVaultService>()
                    .AddSingleton<IVaultManagerService, IOSVaultManagerService>()
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
