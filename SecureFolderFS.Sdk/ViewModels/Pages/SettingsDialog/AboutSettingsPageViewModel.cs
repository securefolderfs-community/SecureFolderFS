using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Pages.SettingsDialog
{
    public sealed class AboutSettingsPageViewModel : BaseSettingsDialogPageViewModel
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
            OpenGitHubRepositoryCommand = new AsyncRelayCommand(OpenGitHubRepository);
            OpenDiscordSocialCommand = new AsyncRelayCommand(OpenDiscordSocial);
            OpenPrivacyPolicyCommand = new AsyncRelayCommand(OpenPrivacyPolicy);
            OpenLogLocationCommand = new AsyncRelayCommand(OpenLogLocation);
        }

        private async Task CopyVersionAsync()
        {
            if (await ClipboardService.IsClipboardAvailableAsync())
            {
                await ClipboardService.SetClipboardDataAsync(new ClipboardTextItemModel(AppVersion));
            }
        }

        private Task OpenGitHubRepository()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS"));
        }

        private Task OpenDiscordSocial()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://discord.com/invite/NrTxXpJ2Zj"));
        }

        private Task OpenPrivacyPolicy()
        {
            return ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/PRIVACY.md"));
        }

        private Task OpenLogLocation()
        {
            return FileExplorerService.OpenAppFolderAsync();
        }
    }
}
