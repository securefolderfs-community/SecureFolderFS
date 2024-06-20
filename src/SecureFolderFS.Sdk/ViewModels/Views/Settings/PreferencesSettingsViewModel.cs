using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    public sealed class PreferencesSettingsViewModel : BaseSettingsViewModel
    {
        public FileSystemBannerViewModel BannerViewModel { get; }

        public PreferencesSettingsViewModel()
        {
            BannerViewModel = new();
            Title = "SettingsPreferences".ToLocalized();
        }

        public bool StartOnSystemStartup
        {
            get => UserSettings.StartOnSystemStartup;
            set => UserSettings.StartOnSystemStartup = value;
        }

        public bool ContinueOnLastVault
        {
            get => UserSettings.ContinueOnLastVault;
            set => UserSettings.ContinueOnLastVault = value;
        }

        public bool OpenFolderOnUnlock
        {
            get => UserSettings.OpenFolderOnUnlock;
            set => UserSettings.OpenFolderOnUnlock = value;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await BannerViewModel.InitAsync(cancellationToken);
        }
    }
}
