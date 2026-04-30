using Android.Content;
using Android.OS;
using CommunityToolkit.Mvvm.Messaging;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Maui.Extensions;
using SecureFolderFS.Maui.Platforms.Android.ServiceImplementation;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using AddService = Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace SecureFolderFS.Maui.Platforms.Android.Helpers
{
    /// <inheritdoc cref="BaseLifecycleHelper"/>
    internal sealed class AndroidLifecycleHelper : BaseLifecycleHelper, IRecipient<VaultUnlockedMessage>
    {
        private bool _isForegroundServiceStarted;
        
        /// <inheritdoc/>
        public override string AppDirectory { get; } = FileSystem.Current.AppDataDirectory;

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize settings
            var settingsFolderPath = Path.Combine(AppDirectory, Constants.FileNames.SETTINGS_FOLDER_NAME);
            var settingsFolder = new SystemFolder(Directory.CreateDirectory(settingsFolderPath));
            ConfigureServices(settingsFolder);

            WeakReferenceMessenger.Default.Register(this);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override void LogExceptionToFile(Exception? ex)
        {
            _ = ex;
        }
        
        /// <inheritdoc/>
        public async void Receive(VaultUnlockedMessage message)
        {
            if (_isForegroundServiceStarted || MainActivity.Instance is null)
                return;
            
            // Start the vault foreground service so it's ready to receive messenger messages
            var serviceIntent = new Intent(MainActivity.Instance, typeof(VaultForegroundService));
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                MainActivity.Instance.StartForegroundService(serviceIntent);
            else
                MainActivity.Instance.StartService(serviceIntent);

            _isForegroundServiceStarted = true;
            
            // Add the initial vault
            var foregroundService = await VaultForegroundService.GetInstanceAsync();
            foregroundService.UnlockedVaults.Add(message.VaultModel);
        }

        /// <inheritdoc/>
        protected override IServiceCollection ConfigureServices(IModifiableFolder settingsFolder)
        {
            return base.ConfigureServices(settingsFolder)
                    //.Override<IIapService, AndroidIapService>(AddService.AddSingleton)
                    .Override<IMediaService, AndroidMediaService>(AddService.AddSingleton)
                    .Override<IShareService, AndroidShareService>(AddService.AddSingleton)
                    .Override<IUpdateService, DebugUpdateService>(AddService.AddSingleton)
                    .Override<ISystemService, AndroidSystemService>(AddService.AddSingleton)
                    .Override<IStorageService, AndroidStorageService>(AddService.AddSingleton)
                    .Override<IApplicationService, AndroidApplicationService>(AddService.AddSingleton)
                    .Override<IFileExplorerService, AndroidFileExplorerService>(AddService.AddSingleton)
                    .Override<ILocalizationService, ResourceLocalizationService>(AddService.AddSingleton)
                    .Override<IVaultFileSystemService, AndroidVaultFileSystemService>(AddService.AddSingleton)
                    .Override<IVaultCredentialsService, AndroidVaultCredentialsService>(AddService.AddSingleton)

                    .WithMauiServices(settingsFolder)
                ;
        }
    }
}
