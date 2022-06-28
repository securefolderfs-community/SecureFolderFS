using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Sdk.ViewModels.Settings.Banners
{
    public sealed class UpdateBannerViewModel : ObservableObject
    {
        private IUpdateService UpdateService { get; } = Ioc.Default.GetRequiredService<IUpdateService>();

        private IApplicationSettingsService ApplicationSettingsService { get; } = Ioc.Default.GetRequiredService<IApplicationSettingsService>();

        public InfoBarViewModel UpdateInfoBar { get; }

        public DateTime LastChecked
        {
            get => ApplicationSettingsService.UpdateLastChecked;
            set => ApplicationSettingsService.UpdateLastChecked = value;
        }

        private string? _UpdateText;
        public string? UpdateText
        {
            get => _UpdateText;
            set => SetProperty(ref _UpdateText, value);
        }

        private bool _AreUpdatesSupported;
        public bool AreUpdatesSupported
        {
            get => _AreUpdatesSupported;
            set => SetProperty(ref _AreUpdatesSupported, value);
        }

        public IAsyncRelayCommand UpdateAppCommand { get; }

        public UpdateBannerViewModel()
        {
            UpdateInfoBar = new();
            UpdateText = "Latest version installed";

            UpdateAppCommand = new AsyncRelayCommand(UpdateAppAsync);
        }

        public async Task ConfigureUpdates()
        {
            var updatesSupported = await UpdateService.AreAppUpdatesSupportedAsync();
            if (!updatesSupported)
            {
                AreUpdatesSupported = false;
                UpdateInfoBar.IsOpen = true;
                UpdateInfoBar.Message = "Updates are not supported for the sideloaded version.";
                UpdateInfoBar.InfoBarSeverity = InfoBarSeverityType.Warning;
                UpdateInfoBar.CanBeClosed = false;
            }
        }

        private async Task UpdateAppAsync()
        {
            LastChecked = DateTime.Now;
            OnPropertyChanged(nameof(LastChecked));

            var isNewUpdateAvailable = await UpdateService.IsNewUpdateAvailableAsync().ConfigureAwait(false);
            if (isNewUpdateAvailable)
            {
                _ = UpdateService.UpdateAppAsync(null);
            }
        }
    }
}
