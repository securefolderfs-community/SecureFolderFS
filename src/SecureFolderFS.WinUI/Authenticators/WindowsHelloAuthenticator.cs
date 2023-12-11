using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Buffers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace SecureFolderFS.WinUI.Authenticators
{
    /// <inheritdoc cref="IAuthenticator"/>
    public sealed class WindowsHelloAuthenticator : IAuthenticator
    {
        /// <inheritdoc/>
        public async Task RevokeAsync(string id, CancellationToken cancellationToken = default)
        {
            await KeyCredentialManager.DeleteAsync(id).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IKey> CreateAsync(string id, byte[] data, CancellationToken cancellationToken)
        {
            var result = await KeyCredentialManager.RequestCreateAsync(id, KeyCredentialCreationOption.ReplaceExisting).AsTask(cancellationToken);
            if (result.Status == KeyCredentialStatus.Success)
                return await SignKeyAsync(result.Credential, data, cancellationToken);

            throw new InvalidOperationException("Failed to create the credential.");
        }

        /// <inheritdoc/>
        public async Task<IKey> SignAsync(string id, byte[] data, CancellationToken cancellationToken)
        {
            var result = await KeyCredentialManager.OpenAsync(id).AsTask(cancellationToken);
            if (result.Status == KeyCredentialStatus.Success)
                return await SignKeyAsync(result.Credential, data, cancellationToken);

            throw new InvalidOperationException("Failed to open the credential.");
        }

        private static async Task<IKey> SignKeyAsync(KeyCredential credential, byte[] data, CancellationToken cancellationToken)
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
