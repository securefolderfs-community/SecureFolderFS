using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;

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
                await ClipboardService.SetClipboardDataAsync(new ClipboardTextItemModel(AppVersion));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task CopySystemVersionAsync(CancellationToken cancellationToken)
        {
            if (await ClipboardService.IsClipboardAvailableAsync())
            {
                var systemVersion = ApplicationService.GetSystemVersion();
                await ClipboardService.SetClipboardDataAsync(new ClipboardTextItemModel(systemVersion));
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenGitHubRepositoryAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS"));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenDiscordSocialAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://discord.com/invite/NrTxXpJ2Zj"));
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private Task OpenPrivacyPolicyAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/PRIVACY.md"));
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
