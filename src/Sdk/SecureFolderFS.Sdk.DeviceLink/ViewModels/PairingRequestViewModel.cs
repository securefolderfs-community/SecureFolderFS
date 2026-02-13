using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.DeviceLink.ViewModels
{
    [Bindable(true)]
    public sealed partial class PairingRequestViewModel : ObservableObject
    {
        [ObservableProperty] private string _DesktopName;
        [ObservableProperty] private string _DesktopType;
        [ObservableProperty] private string _CredentialId;
        [ObservableProperty] private string _VerificationCode;

        /// <summary>
        /// The TaskCompletionSource to signal pairing confirmation result.
        /// </summary>
        public TaskCompletionSource<bool>? ConfirmationTcs { get; }

        public PairingRequestViewModel(string desktopName, string desktopType, string credentialId, string verificationCode, TaskCompletionSource<bool>? confirmationTcs = null)
        {
            DesktopName = desktopName;
            DesktopType = desktopType;
            CredentialId = credentialId;
            VerificationCode = verificationCode;
            ConfirmationTcs = confirmationTcs;
        }

        /// <summary>
        /// Confirms or rejects the pairing request.
        /// </summary>
        /// <param name="confirmed">True to confirm, false to reject.</param>
        public void Confirm(bool confirmed)
        {
            ConfirmationTcs?.TrySetResult(confirmed);
        }
    }
}