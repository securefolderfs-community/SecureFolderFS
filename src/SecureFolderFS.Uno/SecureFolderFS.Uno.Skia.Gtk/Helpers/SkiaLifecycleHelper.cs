using Microsoft.Extensions.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.Skia.Gtk.ServiceImplementation;

namespace SecureFolderFS.Uno.Skia.Gtk.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class SkiaLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            _ = ex;
        }

        /// <inheritdoc/>
        public override void LogException(Exception? ex)
        {
            if (ex.InnerException?.Message.Contains("The remote party") ?? false)
                return;

            base.LogException(ex);
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                //.AddSingleton<IPrinterService, WindowsPrinterService>()
                .AddSingleton<IApplicationService, SkiaApplicationService>()
                .AddSingleton<IVaultManagerService, SkiaVaultManagerService>()
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()
                ;
        }
    }
}
