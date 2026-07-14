using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Helpers;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ISystemService>]
    [Bindable(true)]
    public sealed partial class PreferencesSettingsViewModel : BaseSettingsViewModel
    {
        public FileSystemBannerViewModel BannerViewModel { get; }

        public PreferencesSettingsViewModel()
        {
            ServiceProvider = DI.Default;
            BannerViewModel = new();
            Title = "SettingsPreferences".ToLocalized();
        }

        public bool StartOnSystemStartup
        {
            get => UserSettings.StartOnSystemStartup;
            set
            {
                if (UserSettings.StartOnSystemStartup == value)
                    return;

                UserSettings.StartOnSystemStartup = value;
                _ = ApplyAutoStartAsync(value);
            }
        }

        public bool ReduceToBackground
        {
            get => UserSettings.ReduceToBackground;
            set => UserSettings.ReduceToBackground = value;
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

        public bool AreThumbnailsEnabled
        {
            get => UserSettings.AreThumbnailsEnabled;
            set => UserSettings.AreThumbnailsEnabled = value;
        }

        public bool AreFileExtensionsEnabled
        {
            get => UserSettings.AreFileExtensionsEnabled;
            set => UserSettings.AreFileExtensionsEnabled = value;
        }

        public bool IsAdaptiveLayoutEnabled
        {
            get => UserSettings.IsAdaptiveLayoutEnabled;
            set => UserSettings.IsAdaptiveLayoutEnabled = value;
        }

        public bool IsContentCacheEnabled
        {
            get => UserSettings.IsContentCacheEnabled;
            set => UserSettings.IsContentCacheEnabled = value;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await BannerViewModel.InitAsync(cancellationToken);

            // Reflect auto start changes made outside the app (e.g. in system settings)
            var isAutoStartEnabled = await SafetyHelpers.NoFailureAsync(async () => await SystemService.IsAutoStartEnabledAsync(cancellationToken));
            if (UserSettings.StartOnSystemStartup != isAutoStartEnabled)
            {
                UserSettings.StartOnSystemStartup = isAutoStartEnabled;
                OnPropertyChanged(nameof(StartOnSystemStartup));
            }
        }

        private async Task ApplyAutoStartAsync(bool isEnabled)
        {
            var isApplied = await SafetyHelpers.NoFailureAsync(async () => await SystemService.TrySetAutoStartAsync(isEnabled));
            if (isApplied)
                return;

            // Revert the setting when the platform registration was unsuccessful
            UserSettings.StartOnSystemStartup = !isEnabled;
            OnPropertyChanged(nameof(StartOnSystemStartup));
        }
    }
}
