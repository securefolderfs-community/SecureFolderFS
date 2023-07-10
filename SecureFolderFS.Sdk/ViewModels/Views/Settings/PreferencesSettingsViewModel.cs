using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ISettingsService>]
    public sealed partial class PreferencesSettingsViewModel : BasePageViewModel
    {
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
            ServiceProvider = Ioc.Default;
            BannerViewModel = new();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await BannerViewModel.InitAsync(cancellationToken);
        }
    }
}
