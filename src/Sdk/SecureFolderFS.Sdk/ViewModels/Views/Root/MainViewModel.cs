using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Root
{
    [Inject<ISettingsService>, Inject<ITelemetryService>, Inject<IApplicationService>, Inject<IOverlayService>, Inject<INavigationService>(Name = "RootNavigationService", Visibility = "public")]
    [Bindable(true)]
    public sealed partial class MainViewModel : ObservableObject, IAsyncInitialize
    {
        public IVaultCollectionModel VaultCollectionModel { get; }

        public VaultListViewModel VaultListViewModel { get; }

        public MainViewModel(IVaultCollectionModel vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            VaultCollectionModel = vaultCollectionModel;
            VaultListViewModel = new(vaultCollectionModel);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize
            await Task.WhenAll(SettingsService.TryInitAsync(cancellationToken), VaultCollectionModel.TryInitAsync(cancellationToken));

            // Disable telemetry, if the user opted-out
            if (!SettingsService.UserSettings.IsTelemetryEnabled)
                await TelemetryService.DisableTelemetryAsync();

            // Check if the changelog is available
            if (Version.TryParse(SettingsService.AppSettings.LastVersion, out var lastVersion))
            {
                var currentVersion = ApplicationService.AppVersion;
                if (lastVersion < currentVersion)
                {
                    // Update the last version
                    SettingsService.AppSettings.LastVersion = currentVersion.ToString();
                    _ = SettingsService.AppSettings.SaveAsync(cancellationToken);

                    // Initialize the changelog dialog
                    var changelogOverlay = new ChangelogOverlayViewModel(lastVersion);
                    _ = changelogOverlay.InitAsync(cancellationToken);

                    await OverlayService.ShowAsync(changelogOverlay);
                }
            }
        }
    }
}
