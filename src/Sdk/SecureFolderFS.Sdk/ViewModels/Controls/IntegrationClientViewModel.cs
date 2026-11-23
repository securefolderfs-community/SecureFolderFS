using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Bindable(true)]
    [Inject<ILocalizationService>]
    public sealed partial class IntegrationClientViewModel : ObservableObject
    {
        [ObservableProperty] private string? _DisplayName;
        [ObservableProperty] private string? _ExecutablePath;
        [ObservableProperty] private string? _IdentityDescription;
        [ObservableProperty] private string? _LastUsedDescription;

        public IAsyncRelayCommand<IntegrationClientViewModel> RevokeCommand { get; }

        /// <summary>
        /// Gets the underlying pairing information.
        /// </summary>
        public IntegrationClientInfo ClientInfo { get; }

        public IntegrationClientViewModel(IntegrationClientInfo clientInfo, IAsyncRelayCommand<IntegrationClientViewModel> revokeCommand)
        {
            ServiceProvider = DI.Default;
            ClientInfo = clientInfo;
            RevokeCommand = revokeCommand;
            DisplayName = ClientInfo.DisplayName;
            ExecutablePath = ClientInfo.ExecutablePath;
            IdentityDescription = ClientInfo.IsIdentityVerified && !string.IsNullOrEmpty(ClientInfo.Signer)
                ? "ApiConsentVerifiedPublisher".ToLocalized(ClientInfo.Signer)
                : "ApiClientUnverified".ToLocalized();
            LastUsedDescription = ClientInfo.LastUsedAt is { } lastUsed
                ? LocalizationService.LocalizeDate(lastUsed.LocalDateTime)
                : "ApiClientNeverUsed".ToLocalized();
        }
    }
}
