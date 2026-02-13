using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
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
        private PairingRequestViewModel? _pendingPairingRequest;
        private readonly CredentialsStoreModel _credentialsStoreModel;
        private readonly SynchronizationContext? _synchronizationContext;
        private bool _isInitialized;

        [ObservableProperty] private bool _IsAwaitingPairing;
        [ObservableProperty] private string? _VerificationCode;
        [ObservableProperty] private string? _PendingDesktopName;
        [ObservableProperty] private string? _NewCredentialName;
        [ObservableProperty] private ObservableCollection<CredentialViewModel> _Credentials;

        public bool EnableDeviceLink
        {
            get => SettingsService.UserSettings.EnableDeviceLink;
            set
            {
                if (SettingsService.UserSettings.EnableDeviceLink == value)
                    return;

                SettingsService.UserSettings.EnableDeviceLink = value;
                OnPropertyChanged();
                OnEnablePhoneLinkChanged(value);
            }
        }

        public DeviceLinkCredentialsOverlayViewModel()
        {
            ServiceProvider = DI.Default;
            Credentials = new();
            _synchronizationContext = SynchronizationContext.Current;
            _credentialsStoreModel = new(PropertyStoreService.SecurePropertyStore, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Load saved credentials from the property store
            await _credentialsStoreModel.InitAsync(cancellationToken);

            // Populate the observable collection with loaded credentials
            Credentials.Clear();
            foreach (var credential in _credentialsStoreModel.Credentials)
                Credentials.Add(credential);

            _isInitialized = true;

            // If DeviceLink is enabled, start listening
            if (EnableDeviceLink)
                await StartListeningAsync();
        }

        [RelayCommand]
        private void AcceptPairing()
        {
            _pendingPairingRequest?.Confirm(true);
            _pendingPairingRequest = null;
            IsAwaitingPairing = false;
        }

        [RelayCommand]
        private void RejectPairing()
        {
            _pendingPairingRequest?.Confirm(false);
            _pendingPairingRequest = null;
            IsAwaitingPairing = false;
        }

        [RelayCommand]
        private async Task DeleteCredentialAsync(CredentialViewModel? credential, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(credential?.Id))
                return;

            var messageOverlay = new MessageOverlayViewModel()
            {
                Title = "DeleteDeviceLinkCredential".ToLocalized(),
                Message = "DeleteDeviceLinkCredentialWarning".ToLocalized(credential.DisplayName),
                PrimaryText = "Confirm".ToLocalized(),
                SecondaryText = "Cancel".ToLocalized(),
            };

            var result = await OverlayService.ShowAsync(messageOverlay);
            if (!result.Positive())
                return;

            await _credentialsStoreModel.DeleteCredentialAsync(credential.Id);
            Credentials.Remove(credential);
        }

        private void OnEnablePhoneLinkChanged(bool newValue)
        {
            // Only start/stop listener if already initialized (prevents double-start during init)
            if (_isInitialized)
            {
                if (newValue)
                    _ = StartListeningAsync();
                else
                    StopListening();
            }

            _ = SettingsService.UserSettings.TrySaveAsync();
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
                _pendingPairingRequest = info;
                IsAwaitingPairing = true;
                VerificationCode = info.VerificationCode;
                PendingDesktopName = info.DesktopName;
            };
            _deviceLinkService.AuthenticationRequested += DeviceLink_AuthenticationRequested;
            _deviceLinkService.VerificationCodeReady += (_, code) => VerificationCode = code;
            _deviceLinkService.EnrollmentCompleted += (_, credential) => Credentials.Add(credential);
            _deviceLinkService.Disconnected += (_, _) =>
            {
                _pendingPairingRequest = null;
                IsAwaitingPairing = false;
            };

            await _deviceLinkService.StartListeningAsync();
        }

        private async void DeviceLink_AuthenticationRequested(object? sender, AuthenticationRequestModel e)
        {
            await _synchronizationContext.PostOrExecuteAsync(async _ =>
            {
                var overlayViewModel = new DeviceLinkRequestOverlayViewModel(e);
                await overlayViewModel.InitAsync();
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
