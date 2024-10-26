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
using SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation;
using Windows.Storage;

#if !DEBUG
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SecureFolderFS.UI.Api;
#endif

namespace SecureFolderFS.Uno.Platforms.Windows.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class WindowsLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        protected override string AppDirectory { get; } =
#if WINAPPSDK_PACKAGED
            Directory.GetCurrentDirectory();
#else
            ApplicationData.Current.LocalFolder.Path;
#endif

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
#if !DEBUG
            try
            {
                if (false) // TODO(t): Disable AppCenter
                {
                    // Start AppCenter
                    var appCenterKey = ApiKeys.GetAppCenterKey();
                    if (!string.IsNullOrEmpty(appCenterKey) || !AppCenter.Configured)
                        AppCenter.Start(appCenterKey, typeof(Analytics), typeof(Crashes));
                }
            }
            catch (Exception)
            {
            }
#endif

            var settingsFolderPath = Path.Combine(AppDirectory, SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
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
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                .AddSingleton<ISystemService, WindowsSystemService>()
                .AddSingleton<IPrinterService, WindowsPrinterService>()
                .AddSingleton<IApplicationService, WindowsApplicationService>()
                .AddSingleton<IVaultFileSystemService, WindowsVaultFileSystemService>()
                .AddSingleton<IVaultCredentialsService, WindowsVaultCredentialsService>()

                // ITelemetryService
#if DEBUG
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
#else
                .AddSingleton<ITelemetryService, DebugTelemetryService>()
                // .AddSingleton<ITelemetryService, AppCenterTelemetryService>() // TODO(t): Disable AppCenter
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
                
                .WithUnoServices(settingsFolder)
                ;
        }
    }
}
