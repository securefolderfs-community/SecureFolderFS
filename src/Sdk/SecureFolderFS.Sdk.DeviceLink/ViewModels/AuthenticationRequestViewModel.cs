using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.DeviceLink.Models
{
    [Bindable(true)]
    public sealed partial class AuthenticationRequestViewModel : ObservableObject
    {
        [ObservableProperty] private string _VaultName;
        [ObservableProperty] private string _DesktopName;
        [ObservableProperty] private string _DesktopType;
        [ObservableProperty] private string _CredentialName;

        /// <summary>
        /// The TaskCompletionSource to signal authentication confirmation result.
        /// </summary>
        public TaskCompletionSource<bool>? ConfirmationTcs { get; }

        public AuthenticationRequestViewModel(string vaultName, string desktopName, string desktopType, string credentialName, TaskCompletionSource<bool>? confirmationTcs = null)
        {
            VaultName = vaultName;
            DesktopName = desktopName;
            DesktopType = desktopType;
            CredentialName = credentialName;
            ConfirmationTcs = confirmationTcs;
        }

        /// <summary>
        /// Confirms or rejects the authentication request.
        /// </summary>
        /// <param name="confirmed">True to confirm, false to reject.</param>
        public void Confirm(bool confirmed)
        {
            ConfirmationTcs?.TrySetResult(confirmed);
        }
    }
}
