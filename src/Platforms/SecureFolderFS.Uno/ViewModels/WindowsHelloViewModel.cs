using System;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
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
        protected const int KEY_PART_LENGTH = 128;

        /// <summary>
        /// Gets the unique ID of the vault.
        /// </summary>
        protected string VaultId { get; }

        /// <summary>
        /// Gets the associated folder of the vault.
        /// </summary>
        protected IFolder VaultFolder { get; }

        /// <inheritdoc/>
        public sealed override AuthenticationType Availability { get; } = AuthenticationType.Any;

        protected WindowsHelloViewModel(IFolder vaultFolder, string vaultId)
            : base(Core.Constants.Vault.Authentication.AUTH_WINDOWS_HELLO)
        {
            Title = "WindowsHello".ToLocalized();
            VaultFolder = vaultFolder;
            VaultId = vaultId;
            Icon = "\uEB68";
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
            var encodedVaultIdLength = Encoding.ASCII.GetByteCount(vaultId);
            using var challenge = new SecureKey(KEY_PART_LENGTH + encodedVaultIdLength);
            using var secureRandom = RandomNumberGenerator.Create();

            // Fill the first KEY_PART_LENGTH bytes with secure random data
            secureRandom.GetNonZeroBytes(challenge.Key.AsSpan(0, KEY_PART_LENGTH));

            // Fill the remaining bytes with the ID
            // By using ASCII encoding we get 1:1 byte to char ratio which allows us
            // to use the length of the string ID as part of the SecretKey length above
            var written = Encoding.ASCII.GetBytes(vaultId, challenge.Key.AsSpan(KEY_PART_LENGTH));
            if (written != encodedVaultIdLength)
                throw new FormatException("The allocated buffer and vault ID written bytes amount were different.");
            
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
