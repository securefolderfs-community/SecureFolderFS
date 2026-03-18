using System.Threading.Tasks;
using SecureFolderFS.Sdk.DeviceLink.ViewModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Sdk.DeviceLink.Models
{
    /// <summary>
    /// Holds per-connection session state to avoid conflicts between concurrent connections.
    /// </summary>
    public sealed class ConnectionSession : ICleanable
    {
        public TaskCompletionSource<bool>? PairingConfirmationTcs { get; set; }

        public TaskCompletionSource<bool>? AuthConfirmationTcs { get; set; }

        public CredentialViewModel? CurrentCredential { get; set; }

        public SecureChannelModel? SecureChannel { get; set; }

        /// <inheritdoc/>
        public void Cleanup()
        {
            SafetyHelpers.NoFailure(() => SecureChannel?.Dispose());
            SecureChannel = null;
            PairingConfirmationTcs = null;
            AuthConfirmationTcs = null;
            CurrentCredential = null;
        }
    }
}