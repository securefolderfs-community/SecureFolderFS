using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ViewModels
{
    public sealed partial class WindowsHelloCreationViewModel : WindowsHelloViewModel
    {
        private const int KEY_LENGTH = 128;

        private readonly string _vaultId;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public WindowsHelloCreationViewModel(string vaultId, string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            _vaultId = vaultId;
        }

        [RelayCommand]
        private async Task ProvideCredentialsAsync(CancellationToken cancellationToken)
        {
            var vaultWriter = new VaultWriter(VaultFolder, StreamSerializer.Instance);
            using var challenge = new SecureKey(KEY_LENGTH + _vaultId.Length);
            using var secureRandom = RandomNumberGenerator.Create();

            // Fill the first 128 bytes with secure random data
            secureRandom.GetNonZeroBytes(challenge.Key.AsSpan(0, _vaultId.Length));

            // Fill the remaining bytes with the ID
            // By using ASCII encoding we get 1:1 byte to char ratio which allows us
            // to use the length of the string ID as part of the SecretKey length above
            Encoding.ASCII.GetBytes(_vaultId, challenge.Key.AsSpan(KEY_LENGTH));

            // Write authentication data to the vault
            await vaultWriter.WriteAuthenticationAsync(new()
            {
                Capability = "supportsChallenge",
                Challenge = challenge.Key
            }, cancellationToken);

            var key = await this.TryCreateAsync(_vaultId, challenge.Key, cancellationToken);
            if (key is { Successful: true, Value: not null })
                StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(key.Value));
        }
    }
}
