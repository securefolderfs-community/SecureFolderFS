using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<IOverlayService>, Inject<IClipboardService>, Inject<IApplicationService>, Inject<IFileExplorerService>, Inject<IStorageService>]
    [Bindable(true)]
    public sealed partial class AboutSettingsViewModel : BaseSettingsViewModel
    {
        [ObservableProperty] private string _AppVersion;
        [ObservableProperty] private string _SystemVersion;

        public AboutSettingsViewModel()
        {
            ServiceProvider = DI.Default;
            Title = "SettingsAbout".ToLocalized();
            AppVersion = $"{ApplicationService.AppVersion} ({ApplicationService.Platform})";
            SystemVersion = ApplicationService.GetSystemVersion();
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task OpenGitHubRepositoryAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS"));
        }

        [RelayCommand]
        private Task OpenPrivacyPolicyAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/blob/master/PRIVACY.md"));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenLicensesAsync()
        {
            return OverlayService.ShowAsync(new LicensesOverlayViewModel());
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenOnboardingAsync()
        {
            return OverlayService.ShowAsync(new IntroductionOverlayViewModel());
        }

        [RelayCommand]
        private async Task CopyAppVersionAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(AppVersion, cancellationToken);
        }

        [RelayCommand]
        private async Task CopySystemVersionAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(SystemVersion, cancellationToken);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task OpenChangelogAsync()
        {
            var changelogOverlay = new ChangelogOverlayViewModel(ApplicationService.AppVersion);
            _ = changelogOverlay.InitAsync();

            await OverlayService.ShowAsync(changelogOverlay);
        }

        [RelayCommand]
        private async Task OpenLogLocationAsync(CancellationToken cancellationToken)
        {
            var appFolder = await StorageService.GetAppFolderAsync(cancellationToken);
            await FileExplorerService.TryOpenInFileExplorerAsync(appFolder, cancellationToken);
        }
    }
}
