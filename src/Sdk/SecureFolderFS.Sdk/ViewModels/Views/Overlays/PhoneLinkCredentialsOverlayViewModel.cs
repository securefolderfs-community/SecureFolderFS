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
    [Inject<ISettingsService>, Inject<IPropertyStoreService>]
    public sealed partial class PhoneLinkCredentialsOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private IDeviceLinkService? _deviceLinkService;
        private readonly CredentialsStoreModel _credentialsStoreModel;

        [ObservableProperty] private bool _IsAwaitingPairing;
        [ObservableProperty] private string? _VerificationCode;
        [ObservableProperty] private string? _PendingVaultName;
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

        public PhoneLinkCredentialsOverlayViewModel()
        {
            ServiceProvider = DI.Default;
            Credentials = new();
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
                "iPhone",
                "SecureFolderFS Phone",
                _credentialsStoreModel
            );

            _deviceLinkService.PairingRequested += (_, info) =>
            {
                IsAwaitingPairing = true;
                VerificationCode = info.VerificationCode;
                PendingVaultName = info.VaultName;
            };
            _deviceLinkService.VerificationCodeReady += (_, code) => VerificationCode = code;
            _deviceLinkService.EnrollmentCompleted += (_, credential) => Credentials.Add(credential);
            _deviceLinkService.Disconnected += (_, _) => IsAwaitingPairing = false;

            await _deviceLinkService.StartListeningAsync();
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
