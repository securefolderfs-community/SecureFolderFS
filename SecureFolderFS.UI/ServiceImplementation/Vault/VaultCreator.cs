using SecureFolderFS.Core.Cryptography.Enums;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation.Vault
{
    /// <inheritdoc cref="IVaultCreator"/>
    public sealed class VaultCreator : IVaultCreator
    {
        /// <inheritdoc/>
        public async Task<IDisposable> CreateVaultAsync(IFolder vaultFolder, IPassword password, string nameCipherId, string contentCipherId, CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();

            var nameCipher = GetNameCipher(nameCipherId);
            var contentCipher = GetContentCipher(contentCipherId);

            var superSecret = await creationRoutine
                .SetCredentials(password, null)
                .SetOptions(new() { ContentCipher = contentCipher, FileNameCipher = nameCipher })
                .FinalizeAsync(cancellationToken);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.TryCreateFileAsync(Constants.Vault.VAULT_README_FILENAME, false, cancellationToken);
                if (readmeFile is not null)
                    await readmeFile.WriteAllTextAsync(Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return superSecret;
        }

        private static FileNameCipherScheme GetNameCipher(string cipherId)
        {
            return cipherId switch
            {
                Core.Constants.CipherId.NONE => FileNameCipherScheme.None,
                Core.Constants.CipherId.AES_SIV => FileNameCipherScheme.AES_SIV,
                _ => throw new ArgumentOutOfRangeException(nameof(cipherId))
            };
        }

        private static ContentCipherScheme GetContentCipher(string cipherId)
        {
            return cipherId switch
            {
                Core.Constants.CipherId.AES_CTR_HMAC => ContentCipherScheme.AES_CTR_HMAC,
                Core.Constants.CipherId.AES_GCM => ContentCipherScheme.AES_GCM,
                Core.Constants.CipherId.XCHACHA20_POLY1305 => ContentCipherScheme.XChaCha20_Poly1305,
                _ => throw new ArgumentOutOfRangeException(nameof(cipherId))
            };
        }
    }
}
