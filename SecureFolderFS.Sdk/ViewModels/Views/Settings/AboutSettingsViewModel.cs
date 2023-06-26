using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    public sealed partial class AboutSettingsViewModel : BasePageViewModel
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public string AppVersion { get; }

        public AboutSettingsViewModel()
        {
            AppVersion = ApplicationService.GetAppVersion().ToString();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task CopyAppVersionAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsClipboardAvailableAsync())
                await ClipboardService.SetTextAsync(AppVersion);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task CopySystemVersionAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsClipboardAvailableAsync())
            {
                var systemVersion = ApplicationService.GetSystemVersion();
                await ClipboardService.SetTextAsync(systemVersion);
            }
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
            var viewModel = new ChangelogDialogViewModel();
            _ = viewModel.InitAsync();

            await DialogService.ShowDialogAsync(viewModel);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenLicensesAsync()
        {
            return DialogService.ShowDialogAsync(new LicensesDialogViewModel());
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenLogLocationAsync()
        {
            return FileExplorerService.OpenAppFolderAsync();
        }
    }
}
