using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Banners
{
    [Inject<IUpdateService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class UpdateBannerViewModel : ObservableObject, IAsyncInitialize, IProgress<double>
    {
        [ObservableProperty] private InfoBarViewModel _InfoBarViewModel = new();
        [ObservableProperty] private bool _AreUpdatesSupported;
        [ObservableProperty] private string? _UpdateText;

        public DateTime LastChecked
        {
            get => SettingsService.AppSettings.UpdateLastChecked;
            set => SettingsService.AppSettings.UpdateLastChecked = value;
        }

        public UpdateBannerViewModel()
        {
            ServiceProvider = DI.Default;
            _UpdateText = "LatestVersionInstalled".ToLocalized();
            UpdateService.StateChanged += UpdateService_StateChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            AreUpdatesSupported = await UpdateService.IsSupportedAsync();
            if (!AreUpdatesSupported)
            {
                InfoBarViewModel.IsOpen = true;
                InfoBarViewModel.Message = "Updates are not supported for the sideloaded version.";
                InfoBarViewModel.Severity = ViewSeverityType.Warning;
            }
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            var rounded = (int)Math.Round(value);

            if (rounded == 100)
                UpdateText = "Installing".ToLocalized();
            else
                UpdateText = SafetyHelpers.NoThrowResult(() => string.Format("Downloading".ToLocalized(), rounded)) ?? $"{rounded}%";
        }

        private void UpdateService_StateChanged(object? sender, EventArgs e)
        {
            // Check for any errors
            if (e is not UpdateChangedEventArgs args || (int)args.UpdateState >= 0)
                return;

            InfoBarViewModel.IsOpen = true;
            InfoBarViewModel.Title = "Error".ToLocalized();
            InfoBarViewModel.IsCloseable = true;
            InfoBarViewModel.Message = GetMessageForUpdateState(args.UpdateState);
            InfoBarViewModel.Severity = ViewSeverityType.Error;
        }

        [RelayCommand]
        private async Task UpdateAppAsync(CancellationToken cancellationToken)
        {
            // Update last checked date
            LastChecked = DateTime.Now;
            OnPropertyChanged(nameof(LastChecked));
            await SettingsService.AppSettings.TrySaveAsync(cancellationToken);

            // Check for updates
            var isUpdateAvailable = await UpdateService.IsUpdateAvailableAsync(cancellationToken);
            if (!isUpdateAvailable)
                return;

            var result = await UpdateService.UpdateAsync(this, cancellationToken);
            if (!result.Successful)
            {
                InfoBarViewModel.IsOpen = true;
                InfoBarViewModel.Title = "Error".ToLocalized();
                InfoBarViewModel.IsCloseable = true;
                InfoBarViewModel.Message = result.GetMessage();
                InfoBarViewModel.Severity = ViewSeverityType.Error;
            }
        }

        private static string GetMessageForUpdateState(AppUpdateResultType updateState)
        {
            return updateState switch
            {
                AppUpdateResultType.Completed => "The update has completed successfully",
                AppUpdateResultType.InProgress => "The update is in progress",
                AppUpdateResultType.Canceled => "The update has been canceled",
                AppUpdateResultType.FailedNetworkError => "A network error has occurred",
                AppUpdateResultType.FailedDeviceError => "A device error has occurred",
                AppUpdateResultType.FailedUnknownError => "An unknown error has occurred",
                _ => string.Empty
            };
        }
    }
}
