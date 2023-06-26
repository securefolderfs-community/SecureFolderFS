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
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject, IAsyncInitialize
    {
        public INavigationService HostNavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        private ITelemetryService TelemetryService { get; } = Ioc.Default.GetRequiredService<ITelemetryService>();

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        [ObservableProperty]
        private INotifyPropertyChanged? _AppContentViewModel;

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultCollectionModel = new VaultCollectionModel();

            // Initialize
            await Task.WhenAll(SettingsService.LoadAsync(cancellationToken), vaultCollectionModel.LoadAsync(cancellationToken));

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
                await HostNavigationService.TryNavigateAsync(() => new MainHostViewModel(HostNavigationService, vaultCollectionModel));
            }
            else // Doesn't have vaults
            {
                // Show no vaults screen
                await HostNavigationService.TryNavigateAsync(() => new EmptyHostViewModel(HostNavigationService, vaultCollectionModel));
            }

            // Check if changelog is available
            if (Version.TryParse(SettingsService.AppSettings.LastVersion, out var lastVersion))
            {
                var currentVersion = ApplicationService.GetAppVersion().Version;
                if (lastVersion < currentVersion)
                {
                    // Update the last version
                    SettingsService.AppSettings.LastVersion = currentVersion.ToString();
                    _ = SettingsService.AppSettings.SaveAsync(cancellationToken);

                    // Initialize the changelog dialog
                    var changelogDialog = new ChangelogDialogViewModel(new(lastVersion, ApplicationService.Platform));
                    _ = changelogDialog.InitAsync(cancellationToken);

                    await DialogService.ShowDialogAsync(changelogDialog);
                }
            }
        }
    }
}
