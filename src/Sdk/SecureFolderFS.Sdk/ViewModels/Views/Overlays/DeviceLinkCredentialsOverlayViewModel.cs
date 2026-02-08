using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.PhoneLink.Models;
using SecureFolderFS.Sdk.PhoneLink.Services;
using SecureFolderFS.Sdk.PhoneLink.ViewModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<ISettingsService>, Inject<IPropertyStoreService>, Inject<IOverlayService>]
    public sealed partial class DeviceLinkCredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private DeviceLinkService? _deviceLinkService;
        private readonly CredentialsStoreModel _credentialsStoreModel;
        private readonly SynchronizationContext? _synchronizationContext;

        [ObservableProperty] private bool _IsAwaitingPairing;
        [ObservableProperty] private string? _VerificationCode;
        [ObservableProperty] private string? _PendingDesktopName;
        [ObservableProperty] private string? _NewCredentialName;
        [ObservableProperty] private ObservableCollection<CredentialViewModel> _Credentials;

        public bool EnablePhoneLink
        {
            get => SettingsService.UserSettings.EnablePhoneLink;
            set
            {
                if (SettingsService.UserSettings.EnablePhoneLink == value)
                    return;

                SettingsService.UserSettings.EnablePhoneLink = value;
                OnPropertyChanged();
                OnEnablePhoneLinkChanged(value);
            }
        }

        public DeviceLinkCredentialsOverlayViewModel()
        {
            ServiceProvider = DI.Default;
            Credentials = new();
            _synchronizationContext = SynchronizationContext.Current;
            _credentialsStoreModel = new(PropertyStoreService.InMemoryPropertyStore, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void AcceptPairing()
        {
            _deviceLinkService?.ConfirmPairingRequest(true);
            IsAwaitingPairing = false;
        }

        [RelayCommand]
        private void RejectPairing()
        {
            _deviceLinkService?.ConfirmPairingRequest(false);
            IsAwaitingPairing = false;
        }

        [RelayCommand]
        private async Task DeleteCredentialAsync(CredentialViewModel credential)
        {
            await _credentialsStoreModel.DeleteCredentialAsync(credential.Id);
            Credentials.Remove(credential);
        }

        private void OnEnablePhoneLinkChanged(bool newValue)
        {
            if (newValue)
                _ = StartListeningAsync();
            else
                StopListening();
        }

        private async Task StartListeningAsync()
        {
            _deviceLinkService = new DeviceLinkService(
                Environment.MachineName,
                "SecureFolderFS Phone",
                _credentialsStoreModel
            );

            _deviceLinkService.PairingRequested += (_, info) =>
            {
                IsAwaitingPairing = true;
                VerificationCode = info.VerificationCode;
                PendingDesktopName = info.DesktopName;
            };
            _deviceLinkService.AuthenticationRequested += DeviceLink_AuthenticationRequested;
            _deviceLinkService.VerificationCodeReady += (_, code) => VerificationCode = code;
            _deviceLinkService.EnrollmentCompleted += (_, credential) => Credentials.Add(credential);
            _deviceLinkService.Disconnected += (_, _) => IsAwaitingPairing = false;

            await _deviceLinkService.StartListeningAsync();
        }

        private async void DeviceLink_AuthenticationRequested(object? sender, AuthenticationRequestModel e)
        {
            if (sender is not DeviceLinkService deviceLinkService)
                return;

            await _synchronizationContext.PostOrExecuteAsync(async _ =>
            {
                var overlayViewModel = new DeviceLinkRequestOverlayViewModel(deviceLinkService, e);
                await OverlayService.ShowAsync(overlayViewModel);
            });
        }

        private void StopListening()
        {
            _deviceLinkService?.StopListening();
            _deviceLinkService?.Dispose();
            _deviceLinkService = null;
            IsAwaitingPairing = false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            StopListening();
        }
    }
}
