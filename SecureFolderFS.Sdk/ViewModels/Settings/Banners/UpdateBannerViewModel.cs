using System;
using System.Threading;
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
    public sealed partial class UpdateBannerViewModel : ObservableObject
    {
        private IGeneralSettingsService GeneralSettingsService { get; } = Ioc.Default.GetRequiredService<IGeneralSettingsService>();

        private IUpdateService UpdateService { get; } = Ioc.Default.GetRequiredService<IUpdateService>();

        public InfoBarViewModel UpdateInfoBar { get; }

        [ObservableProperty]
        private string? _UpdateText;

        [ObservableProperty]
        private bool _AreUpdatesSupported;

        public DateTime LastChecked
        {
            get => GeneralSettingsService.UpdateLastChecked;
            set => GeneralSettingsService.UpdateLastChecked = value;
        }

        public UpdateBannerViewModel()
        {
            UpdateInfoBar = new();
            UpdateText = "Latest version installed";
        }

        public async Task ConfigureUpdates()
        {
            var updatesSupported = await UpdateService.IsSupportedAsync();
            var isInitialized = updatesSupported && await UpdateService.InitializeAsync();

            if (!updatesSupported || !isInitialized)
            {
                AreUpdatesSupported = false;
                UpdateInfoBar.IsOpen = true;
                UpdateInfoBar.Message = "Updates are not supported for the sideloaded version.";
                UpdateInfoBar.InfoBarSeverity = InfoBarSeverityType.Warning;
                UpdateInfoBar.CanBeClosed = false;
            }
        }

        [RelayCommand]
        private async Task UpdateAppAsync(CancellationToken cancellationToken)
        {
            LastChecked = DateTime.Now;
            OnPropertyChanged(nameof(LastChecked));

            var isUpdateAvailable = await UpdateService.IsUpdateAvailableAsync();
            if (isUpdateAvailable)
                _ = UpdateService.UpdateAsync(null, cancellationToken);
        }
    }
}
