using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using Windows.Security.Credentials;

namespace SecureFolderFS.Uno.ViewModels
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    public abstract class WindowsHelloViewModel : AuthenticationViewModel
    {
        protected const int KEY_LENGTH = 128;

        protected WindowsHelloViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            DisplayName = "Windows Hello";
        }

        /// <inheritdoc/>
        public override async Task RevokeAsync(string id, CancellationToken cancellationToken = default)
        {
            await KeyCredentialManager.DeleteAsync(id).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var result = await KeyCredentialManager.RequestCreateAsync(id, KeyCredentialCreationOption.ReplaceExisting).AsTask(cancellationToken);
            if (result.Status != KeyCredentialStatus.Success)
                throw new InvalidOperationException("Failed to create the credential.");
            
            return await CreateSignatureAsync(result.Credential, data, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IKey> SignAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var result = await KeyCredentialManager.OpenAsync(id).AsTask(cancellationToken);
            if (result.Status != KeyCredentialStatus.Success)
                throw new InvalidOperationException("Failed to open the credential.");
            
            return await CreateSignatureAsync(result.Credential, data, cancellationToken);
        }

        protected static SecretKey GenerateChallenge(string vaultId)
        {
            using var challenge = new SecureKey(KEY_LENGTH + vaultId.Length);
            using var secureRandom = RandomNumberGenerator.Create();

            // Fill the first 128 bytes with secure random data
            secureRandom.GetNonZeroBytes(challenge.Key.AsSpan(0, vaultId.Length));

            // Fill the remaining bytes with the ID
            // By using ASCII encoding we get 1:1 byte to char ratio which allows us
            // to use the length of the string ID as part of the SecretKey length above
            Encoding.ASCII.GetBytes(vaultId, challenge.Key.AsSpan(KEY_LENGTH));

            // Return a copy of the challenge since the original version is being disposed of
            return challenge.CreateCopy();
        }

        protected static async Task<SecretKey> CreateSignatureAsync(KeyCredential credential, byte[] data, CancellationToken cancellationToken)
        {
            var buffer = data.AsBuffer();
            var signed = await credential.RequestSignAsync(buffer).AsTask(cancellationToken);
            if (signed.Status != KeyCredentialStatus.Success)
                throw new InvalidOperationException("Failed to sign the data.");
            
            var secretKey = new SecureKey((int)signed.Result.Length);
            signed.Result.CopyTo(secretKey.Key);

            return secretKey;
        }
    }
}
