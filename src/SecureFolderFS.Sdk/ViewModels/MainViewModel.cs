using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels
{
    [Inject<ISettingsService>, Inject<ITelemetryService>, Inject<IApplicationService>, Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class MainViewModel : ObservableObject, IAsyncInitialize
    {
        public IVaultCollectionModel VaultCollectionModel { get; } = new VaultCollectionModel();

        public MainViewModel()
        {
            ServiceProvider = Ioc.Default;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize
            await Task.WhenAll(SettingsService.TryLoadAsync(cancellationToken), VaultCollectionModel.TryLoadAsync(cancellationToken));

            // Disable telemetry, if the user opted-out
            if (!SettingsService.UserSettings.IsTelemetryEnabled)
                await TelemetryService.DisableTelemetryAsync();

            // Check if the user was introduced (OOBE)
            if (false && !SettingsService.AppSettings.IsIntroduced)
            {
                var dialogService = Ioc.Default.GetRequiredService<IOverlayService>();
                var dialogResult = await OverlayService.ShowAsync(new AgreementDialogViewModel());
                if (dialogResult is IResult<DialogOption> { Value: DialogOption.Primary } || dialogResult.Successful)
                {
                    SettingsService.AppSettings.IsIntroduced = true;
                    await SettingsService.SaveAsync(cancellationToken);
                }
            }

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
