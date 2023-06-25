using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject, IAsyncInitialize
    {
        public INavigationService HostNavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        [ObservableProperty]
        private INotifyPropertyChanged? _AppContentViewModel;

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultCollectionModel = new VaultCollectionModel();
            var settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            var telemetryService = Ioc.Default.GetRequiredService<ITelemetryService>();

            // Initialize
            await Task.WhenAll(settingsService.LoadAsync(cancellationToken), vaultCollectionModel.LoadAsync(cancellationToken));

            // Disable telemetry, if the user opted-out
            if (!settingsService.UserSettings.IsTelemetryEnabled)
                await telemetryService.DisableTelemetryAsync();

            // Check if the user was introduced (OOBE)
            if (false && !settingsService.AppSettings.IsIntroduced)
            {
                var dialogService = Ioc.Default.GetRequiredService<IDialogService>();
                var dialogResult = await dialogService.ShowDialogAsync(new AgreementDialogViewModel());
                if (dialogResult is IResult<DialogOption> { Value: DialogOption.Primary } || dialogResult.Successful)
                {
                    settingsService.AppSettings.IsIntroduced = true;
                    await settingsService.SaveAsync(cancellationToken);
                }
            }

            // Load main screen
            if (!vaultCollectionModel.IsEmpty()) // Has vaults
            {
                // Show main app screen
                await HostNavigationService.TryNavigateAsync(() => new MainHostViewModel(HostNavigationService, vaultCollectionModel));
            }
            else // Doesn't have vaults
            {
                // Show no vaults screen
                await HostNavigationService.TryNavigateAsync(() => new EmptyHostViewModel(HostNavigationService, vaultCollectionModel));
            }
        }
    }
}
