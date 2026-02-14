using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.DeviceLink.Models
{
    [Bindable(true)]
    public sealed partial class AuthenticationRequestViewModel(TaskCompletionSource<bool> confirmationTcs)
        : ObservableObject
    {
        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private string? _DesktopName;
        [ObservableProperty] private string? _DesktopType;
        [ObservableProperty] private string? _CredentialId;
        [ObservableProperty] private string? _CredentialName;

        /// <summary>
        /// The TaskCompletionSource to signal authentication confirmation result.
        /// </summary>
        public TaskCompletionSource<bool> ConfirmationTcs { get; } = confirmationTcs;

        /// <summary>
        /// Confirms or rejects the authentication request.
        /// </summary>
        /// <param name="confirmed">True to confirm, false to reject.</param>
        public void Confirm(bool confirmed)
        {
            ConfirmationTcs.TrySetResult(confirmed);
        }
    }
}
