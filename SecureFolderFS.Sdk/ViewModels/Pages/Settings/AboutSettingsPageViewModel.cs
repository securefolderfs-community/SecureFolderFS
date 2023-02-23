using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Settings
{
    public sealed partial class AboutSettingsPageViewModel : ObservableObject
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        public string AppVersion { get; }

        public AboutSettingsPageViewModel()
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
        private Task OpenLogLocationAsync()
        {
            return FileExplorerService.OpenAppFolderAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenLicensesDialogAsync()
        {
            return DialogService.ShowDialogAsync(new LicensesDialogViewModel());
        }
    }
}
