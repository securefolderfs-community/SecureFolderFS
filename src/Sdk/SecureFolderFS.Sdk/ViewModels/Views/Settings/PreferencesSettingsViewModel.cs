using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Banners;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ISystemService>, Inject<ILocalIntegrationsService>(Optionality = "optional")]
    [Bindable(true)]
    public sealed partial class PreferencesSettingsViewModel : BaseSettingsViewModel
    {
        public FileSystemBannerViewModel BannerViewModel { get; }

        public bool AreIntegrationsSupported { get; }

        /// <summary>
        /// Gets the applications currently authorized to use the local integration API.
        /// </summary>
        public ObservableCollection<IntegrationClientViewModel> ConnectedApps { get; }

        public PreferencesSettingsViewModel()
        {
            ServiceProvider = DI.Default;
            BannerViewModel = new();
            ConnectedApps = new();
            AreIntegrationsSupported = LocalIntegrationsService is not null;
            Title = "SettingsPreferences".ToLocalized();
        }

        public bool EnableLocalIntegrations
        {
            get => UserSettings.EnableLocalIntegrations;
            set
            {
                if (UserSettings.EnableLocalIntegrations == value)
                    return;

                _ = ApplyIntegrationsAsync(value);
            }
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

            RefreshConnectedApps();

            // Reflect auto-start changes made outside the app (e.g., in system settings)
            var isAutoStartEnabled = await SafetyHelpers.NoFailureAsync(async () => await SystemService.IsAutoStartEnabledAsync(cancellationToken));
            if (UserSettings.StartOnSystemStartup != isAutoStartEnabled)
            {
                UserSettings.StartOnSystemStartup = isAutoStartEnabled;
                OnPropertyChanged(nameof(StartOnSystemStartup));
            }
        }

        [RelayCommand]
        private async Task RevokeAllAppsAsync()
        {
            if (LocalIntegrationsService is null)
                return;

            await LocalIntegrationsService.RevokeAllClientsAsync();
            ConnectedApps.Clear();
        }

        [RelayCommand]
        private async Task RevokeAppAsync(IntegrationClientViewModel client, CancellationToken cancellationToken)
        {
            if (LocalIntegrationsService is null)
                return;

            await LocalIntegrationsService.RevokeClientAsync(client.ClientInfo.Id, cancellationToken);
            ConnectedApps.Remove(client);
        }

        private async Task ApplyIntegrationsAsync(bool isEnabled)
        {
            if (LocalIntegrationsService is null)
                return;

            try
            {
                await LocalIntegrationsService.SetEnabledAsync(isEnabled);
            }
            catch (Exception)
            {
                // The endpoint could not be bound, so leave the setting reflecting what actually happened
                UserSettings.EnableLocalIntegrations = !isEnabled;
            }

            OnPropertyChanged(nameof(EnableLocalIntegrations));
            RefreshConnectedApps();
        }

        private void RefreshConnectedApps()
        {
            ConnectedApps.Clear();
            if (LocalIntegrationsService is null)
                return;

            foreach (var client in LocalIntegrationsService.GetClients())
                ConnectedApps.Add(new IntegrationClientViewModel(client, RevokeAppCommand));
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
