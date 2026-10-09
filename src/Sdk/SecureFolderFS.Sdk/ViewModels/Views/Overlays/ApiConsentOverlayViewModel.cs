using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    /// <summary>
    /// Asks the user whether an application may connect to the local integration API.
    /// </summary>
    [Bindable(true)]
    public sealed partial class ApiConsentOverlayViewModel : OverlayViewModel
    {
        /// <summary>
        /// Gets the client-supplied application name. Untrusted, and never shown without context.
        /// </summary>
        [ObservableProperty] private string _ClientName;

        /// <summary>
        /// Gets the path of the connecting executable, when it could be determined.
        /// </summary>
        [ObservableProperty] private string? _ExecutablePath;

        /// <summary>
        /// Gets the verified publisher, when the platform could establish one.
        /// </summary>
        [ObservableProperty] private string? _Signer;

        /// <summary>
        /// Gets a value indicating whether the caller's publisher was cryptographically verified.
        /// </summary>
        [ObservableProperty] private bool _IsIdentityVerified;

        /// <summary>
        /// Gets the sentence describing what could be established about the caller's identity.
        /// </summary>
        [ObservableProperty] private string _IdentityDescription;

        /// <summary>
        /// Gets the human-readable description of what the caller is asking to do.
        /// </summary>
        public ObservableCollection<string> RequestedPermissions { get; }

        public ApiConsentOverlayViewModel(
            string clientName,
            string? executablePath,
            string? signer,
            bool isIdentityVerified,
            IEnumerable<string> requestedPermissions)
        {
            ClientName = clientName;
            ExecutablePath = executablePath;
            Signer = signer;
            IsIdentityVerified = isIdentityVerified;
            RequestedPermissions = new(requestedPermissions);

            Title = "ApiConsentTitle".ToLocalized();
            PrimaryText = "Allow".ToLocalized();
            SecondaryText = "Deny".ToLocalized();
            CanContinue = true;
            CanCancel = true;

            IdentityDescription = isIdentityVerified && !string.IsNullOrEmpty(signer)
                ? "ApiConsentVerifiedPublisher".ToLocalized(signer)
                : "ApiConsentUnverifiedPublisher".ToLocalized();
        }
    }
}
