using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<IOverlayService>, Inject<IClipboardService>, Inject<IApplicationService>, Inject<IFileExplorerService>]
    public sealed partial class AboutSettingsViewModel : BasePageViewModel
    {
        public string AppVersion { get; }

        public AboutSettingsViewModel()
        {
            ServiceProvider = Ioc.Default;
            AppVersion = $"{ApplicationService.AppVersion} ({ApplicationService.Platform})";
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task CopyAppVersionAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(AppVersion, cancellationToken);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task CopySystemVersionAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsSupportedAsync())
                await ClipboardService.SetTextAsync(ApplicationService.GetSystemVersion(), cancellationToken);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenGitHubRepositoryAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS"));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenPrivacyPolicyAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/blob/master/PRIVACY.md"));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task OpenChangelogAsync()
        {
            var viewModel = new ChangelogDialogViewModel(ApplicationService.AppVersion);
            _ = viewModel.InitAsync();

            await OverlayService.ShowAsync(viewModel);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenLicensesAsync()
        {
            return OverlayService.ShowAsync(new LicensesDialogViewModel());
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenLogLocationAsync()
        {
            return FileExplorerService.OpenAppFolderAsync();
        }
    }
}
