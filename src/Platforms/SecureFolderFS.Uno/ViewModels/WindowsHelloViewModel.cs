using System;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using Windows.Security.Credentials;

namespace SecureFolderFS.Uno.ViewModels
{
    /// <inheritdoc cref="AuthenticationViewModel"/>
    [Bindable(true)]
    public abstract class WindowsHelloViewModel : AuthenticationViewModel
    {
        /// <summary>
        /// Gets the unique ID of the vault.
        /// </summary>
        protected string VaultId { get; }

        /// <summary>
        /// Gets the associated folder of the vault.
        /// </summary>
        protected IFolder VaultFolder { get; }

        /// <inheritdoc/>
        public sealed override bool CanComplement { get; } = true;

        /// <inheritdoc/>
        public sealed override AuthenticationStage Availability { get; } = AuthenticationStage.Any;

        protected WindowsHelloViewModel(IFolder vaultFolder, string vaultId)
            : base(Core.Constants.Vault.Authentication.AUTH_WINDOWS_HELLO)
        {
            Title = "WindowsHello".ToLocalized();
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }

        /// <inheritdoc/>
        public override async Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            id ??= VaultId;
            await KeyCredentialManager.DeleteAsync(id).AsTask(cancellationToken);
            if (VaultFolder is not IModifiableFolder modifiableFolder)
                return;

            var authenticationFile = await modifiableFolder.GetFileByNameAsync($"{Id}{Core.Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);
            await modifiableFolder.DeleteAsync(authenticationFile, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IKey> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var result = await KeyCredentialManager.RequestCreateAsync(id, KeyCredentialCreationOption.ReplaceExisting).AsTask(cancellationToken);
            if (result.Status != KeyCredentialStatus.Success)
                throw new InvalidOperationException("Failed to create the credential.");

            return await MakeSignatureAsync(result.Credential, data, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IKey> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var result = await KeyCredentialManager.OpenAsync(id).AsTask(cancellationToken);
            if (result.Status != KeyCredentialStatus.Success)
                throw new InvalidOperationException("Failed to open the credential.");

            return await MakeSignatureAsync(result.Credential, data, cancellationToken);
        }

        protected static async Task<SecretKey> MakeSignatureAsync(KeyCredential credential, byte[] data, CancellationToken cancellationToken)
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
