using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    public sealed class PreferencesSettingsViewModel : BasePageViewModel
    {
        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public FileSystemBannerViewModel BannerViewModel { get; }

        public bool StartOnSystemStartup
        {
            get => SettingsService.UserSettings.StartOnSystemStartup;
            set => SettingsService.UserSettings.StartOnSystemStartup = value;
        }

        public bool ContinueOnLastVault
        {
            get => SettingsService.UserSettings.ContinueOnLastVault;
            set => SettingsService.UserSettings.ContinueOnLastVault = value;
        }

        public bool OpenFolderOnUnlock
        {
            get => SettingsService.UserSettings.OpenFolderOnUnlock;
            set => SettingsService.UserSettings.OpenFolderOnUnlock = value;
        }

        public PreferencesSettingsViewModel()
        {
            BannerViewModel = new();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await BannerViewModel.InitAsync(cancellationToken);
        }
    }
}
