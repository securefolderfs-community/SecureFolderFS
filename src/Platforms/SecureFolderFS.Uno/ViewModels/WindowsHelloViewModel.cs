using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace SecureFolderFS.Uno.ViewModels
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    public abstract class WindowsHelloViewModel : AuthenticationViewModel
    {
        private IKey? _key;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        protected WindowsHelloViewModel(string id, IFolder vaultFolder)
            : base(id, vaultFolder)
        {
            DisplayName = "Windows Hello";
        }

        /// <inheritdoc/>
        public override IKey? RetrieveKey()
        {
            return _key;
        }

        /// <inheritdoc/>
        public override async Task RevokeAsync(string id, CancellationToken cancellationToken = default)
        {
            await KeyCredentialManager.DeleteAsync(id).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            var result = await KeyCredentialManager.RequestCreateAsync(id, KeyCredentialCreationOption.ReplaceExisting).AsTask(cancellationToken);
            if (result.Status == KeyCredentialStatus.Success)
            {
                _key?.Dispose();
                _key = await CreateSignatureAsync(result.Credential, data, cancellationToken);
                StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(_key));

                return _key;
            }

            throw new InvalidOperationException("Failed to create the credential.");
        }

        /// <inheritdoc/>
        public override async Task<IKey> SignAsync(string id, byte[] data, CancellationToken cancellationToken = default)
        {
            var result = await KeyCredentialManager.OpenAsync(id).AsTask(cancellationToken);
            if (result.Status == KeyCredentialStatus.Success)
            {
                _key?.Dispose();
                _key = await CreateSignatureAsync(result.Credential, data, cancellationToken);
                StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(_key));

                return _key;
            }

            throw new InvalidOperationException("Failed to open the credential.");
        }

        private static async Task<IKey> CreateSignatureAsync(KeyCredential credential, byte[] data, CancellationToken cancellationToken)
        {
            var buffer = data.AsBuffer();
            var signed = await credential.RequestSignAsync(buffer).AsTask(cancellationToken);
            if (signed.Status != KeyCredentialStatus.Success)
                throw new InvalidOperationException("Failed to sign the data.");

            var secretKey = new SecureKey((int)signed.Result.Length);
            signed.Result.CopyTo(secretKey.Key);

            return secretKey;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _key?.Dispose();
        }
    }
}
