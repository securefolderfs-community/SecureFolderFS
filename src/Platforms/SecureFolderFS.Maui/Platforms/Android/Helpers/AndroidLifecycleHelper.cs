using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Platforms.Android.ServiceImplementation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Maui.Platforms.Android.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class AndroidLifecycleHelper : BaseLifecycleHelper
    {
        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Request permissions
            //await RequestPermissionsAsync<Permissions.StorageWrite>();

            // Initialize settings
            var settingsFolderPath = Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, SecureFolderFS.UI.Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);
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
                    //.AddSingleton<IApplicationService, SkiaApplicationService>()
                    .AddSingleton<IVaultService, AndroidVaultService>()
                    .AddSingleton<IVaultManagerService, AndroidVaultManagerService>()
                    .AddSingleton<IStorageService, AndroidStorageService>()
                    .AddSingleton<IFileExplorerService, AndroidFileExplorerService>()
                    .AddSingleton<ITelemetryService, DebugTelemetryService>()
                    .AddSingleton<IIapService, DebugIapService>()
                    .AddSingleton<IUpdateService, DebugUpdateService>()
                    .AddSingleton<ILocalizationService, ResourceLocalizationService>()
                
                    .WithMauiServices(settingsFolder)
                ;
        }

        private async Task RequestPermissionsAsync<TPermission>()
            where TPermission : Permissions.BasePermission, new()
        {
            var permissionStatus = await Permissions.CheckStatusAsync<TPermission>();
            switch (permissionStatus)
            {
                case PermissionStatus.Denied:
                    if (Permissions.ShouldShowRationale<TPermission>())
                    {
                        await Shell.Current.DisplayAlert("Action required",
                            "For SecureFolderFS to function correctly, you'll need to grant the storage permission.",
                            "Ok");
                    }

                    if (await Permissions.RequestAsync<TPermission>() != PermissionStatus.Granted)
                    {
                        await Toast.Make("Storage permissions are required for SecureFolderFS", ToastDuration.Long).Show();
                        Application.Current?.Quit();
                    }

                    return;

                case PermissionStatus.Disabled:
                case PermissionStatus.Restricted:
                    await Toast.Make("Storage permissions are required for SecureFolderFS", ToastDuration.Long).Show();
                    Application.Current?.Quit();
                    return;

                case PermissionStatus.Granted:
                    return;

                default:
                case PermissionStatus.Limited:
                case PermissionStatus.Unknown:
                    return;
            }
        }
    }
}
