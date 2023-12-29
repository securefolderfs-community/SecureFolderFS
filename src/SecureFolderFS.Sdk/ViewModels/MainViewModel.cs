using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    [Inject<ISettingsService>, Inject<ITelemetryService>, Inject<IApplicationService>, Inject<IDialogService>, Inject<INavigationService>(Visibility = "public", Name = "HostNavigationService")]
    public sealed partial class MainViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty]
        private INotifyPropertyChanged? _AppContentViewModel;

        public MainViewModel()
        {
            ServiceProvider = Ioc.Default;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultCollectionModel = new VaultCollectionModel();

            // Initialize
            await Task.WhenAll(SettingsService.TryLoadAsync(cancellationToken), vaultCollectionModel.TryLoadAsync(cancellationToken));

            // Disable telemetry, if the user opted-out
            if (!SettingsService.UserSettings.IsTelemetryEnabled)
                await TelemetryService.DisableTelemetryAsync();

            // Check if the user was introduced (OOBE)
            if (false && !SettingsService.AppSettings.IsIntroduced)
            {
                var dialogService = Ioc.Default.GetRequiredService<IDialogService>();
                var dialogResult = await dialogService.ShowDialogAsync(new AgreementDialogViewModel());
                if (dialogResult is IResult<DialogOption> { Value: DialogOption.Primary } || dialogResult.Successful)
                {
                    SettingsService.AppSettings.IsIntroduced = true;
                    await SettingsService.SaveAsync(cancellationToken);
                }
            }

            // Load main screen
            if (!vaultCollectionModel.IsEmpty()) // Has vaults
            {
                // Show main app screen
                await HostNavigationService.TryNavigateAsync(() => new MainHostViewModel(HostNavigationService, vaultCollectionModel), false);
            }
            else // Doesn't have vaults
            {
                // Show no vaults screen
                await HostNavigationService.TryNavigateAsync(() => new EmptyHostViewModel(HostNavigationService, vaultCollectionModel), false);
            }

            // Check if the changelog is available
            if (Version.TryParse(SettingsService.AppSettings.LastVersion, out var lastVersion) && false) // TODO: Removed due to markdown being unavailable
            {
                var currentVersion = ApplicationService.AppVersion;
                if (lastVersion < currentVersion)
                {
                    // Update the last version
                    SettingsService.AppSettings.LastVersion = currentVersion.ToString();
                    _ = SettingsService.AppSettings.SaveAsync(cancellationToken);

                    // Initialize the changelog dialog
                    var changelogDialog = new ChangelogDialogViewModel(lastVersion);
                    _ = changelogDialog.InitAsync(cancellationToken);

                    await DialogService.ShowDialogAsync(changelogDialog);
                }
            }
        }
    }
}
