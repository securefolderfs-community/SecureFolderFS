using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Settings
{
    public sealed class AboutSettingsPageViewModel : ObservableObject
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        public string AppVersion { get; }

        public IAsyncRelayCommand CopyVersionCommand { get; }

        public IAsyncRelayCommand OpenGitHubRepositoryCommand { get; }

        public IAsyncRelayCommand OpenDiscordSocialCommand { get; }

        public IAsyncRelayCommand OpenPrivacyPolicyCommand { get; }

        public IAsyncRelayCommand OpenLogLocationCommand { get; }

        public AboutSettingsPageViewModel()
        {
            AppVersion = ApplicationService.GetAppVersion().ToString();

            CopyVersionCommand = new AsyncRelayCommand(CopyVersionAsync);
            OpenGitHubRepositoryCommand = new AsyncRelayCommand(OpenGitHubRepositoryAsync);
            OpenDiscordSocialCommand = new AsyncRelayCommand(OpenDiscordSocialAsync);
            OpenPrivacyPolicyCommand = new AsyncRelayCommand(OpenPrivacyPolicyAsync);
            OpenLogLocationCommand = new AsyncRelayCommand(OpenLogLocationAsync);
        }

        private async Task CopyVersionAsync()
        {
            if (await ClipboardService.IsClipboardAvailableAsync())
            {
                await ClipboardService.SetClipboardDataAsync(new ClipboardTextItemModel(AppVersion));
            }
        }

        private Task OpenGitHubRepositoryAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS"));
        }

        private Task OpenDiscordSocialAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://discord.com/invite/NrTxXpJ2Zj"));
        }

        private Task OpenPrivacyPolicyAsync()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/PRIVACY.md"));
        }

        private Task OpenLogLocationAsync()
        {
            return FileExplorerService.OpenAppFolderAsync();
        }
    }
}
