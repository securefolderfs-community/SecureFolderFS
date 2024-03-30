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
using SecureFolderFS.Uno.Windows.ServiceImplementation;
using Windows.Storage;

#if !DEBUG
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.UI.Api;
#endif

namespace SecureFolderFS.Uno.Windows.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class WindowsLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
#if !DEBUG
            try
            {
                // Start AppCenter
                var appCenterKey = ApiKeys.GetAppCenterKey();
                if (!string.IsNullOrEmpty(appCenterKey) || !AppCenter.Configured)
                    AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
            }
            catch (Exception)
            {
            }
#endif

#if UNPACKAGED
            var settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
#else
            var settingsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
#endif

            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                .AddSingleton<IPrinterService, WindowsPrinterService>()
                .AddSingleton<IVaultService, WindowsVaultService>()
                .AddSingleton<IVaultManagerService, WindowsVaultManagerService>()
                .AddSingleton<IApplicationService, WindowsApplicationService>()

                // ITelemetryService
#if DEBUG
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
#else
                .AddSingleton<ITelemetryService, AppCenterTelemetryService>()
#endif

                // IIapService, IUpdateService
#if DEBUG || UNPACKAGED
                .AddSingleton<IIapService, DebugIapService>()
                .AddSingleton<IUpdateService, DebugUpdateService>()
#else
                .AddSingleton<IIapService, DebugIapService>() // .AddSingleton<IIapService, MicrosoftStoreIapService>() // TODO: Change in the future
                .AddSingleton<IUpdateService, MicrosoftStoreUpdateService>()
#endif

                // ILocalizationService
#if UNPACKAGED
                .AddSingleton<ILocalizationService, ResourceLocalizationService>()
#else
                .AddSingleton<ILocalizationService, WindowsLocalizationService>()
#endif
                ;
        }

        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            _ = ex;
        }
    }
}
