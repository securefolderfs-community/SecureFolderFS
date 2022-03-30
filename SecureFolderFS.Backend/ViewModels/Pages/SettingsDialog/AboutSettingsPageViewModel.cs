using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Backend.Services;

namespace SecureFolderFS.Backend.ViewModels.Pages.SettingsDialog
{
    public sealed class AboutSettingsPageViewModel : BaseSettingsDialogPageViewModel
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        private IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        public string AppVersion
        {
             get => ApplicationService.GetAppVersion().ToString();
        }

        public IRelayCommand CopyVersionCommand { get; }

        public IAsyncRelayCommand OpenGitHubRepositoryCommand { get; }

        public IAsyncRelayCommand OpenDiscordSocialCommand { get; }

        public IAsyncRelayCommand OpenPrivacyPolicyCommand { get; }

        public IAsyncRelayCommand OpenLogLocationCommand { get; }

        public AboutSettingsPageViewModel()
        {
            CopyVersionCommand = new RelayCommand(CopyVersion);
            OpenGitHubRepositoryCommand = new AsyncRelayCommand(OpenGitHubRepository);
            OpenDiscordSocialCommand = new AsyncRelayCommand(OpenDiscordSocial);
            OpenPrivacyPolicyCommand = new AsyncRelayCommand(OpenPrivacyPolicy);
            OpenLogLocationCommand = new AsyncRelayCommand(OpenLogLocation);
        }

        private void CopyVersion()
        {
            ClipboardService.SetData(AppVersion);
        }

        private async Task OpenGitHubRepository()
        {
            await ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS"));
        }

        private async Task OpenDiscordSocial()
        {
            await ApplicationService.OpenUriAsync(new Uri("https://discord.com/invite/NrTxXpJ2Zj"));
        }

        private async Task OpenPrivacyPolicy()
        {
            await ApplicationService.OpenUriAsync(new Uri("https://github.com/securefolderfs-community/SecureFolderFS/PRIVACY.md"));
        }

        private async Task OpenLogLocation()
        {
            await ApplicationService.OpenAppFolderAsync();
        }
    }
}
