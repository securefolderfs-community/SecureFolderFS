using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SecureFolderFS.Core.Constants;

namespace SecureFolderFS.UI.ServiceImplementation.Vault
{
    /// <inheritdoc cref="IVaultCreator"/>
    public sealed class VaultCreator : IVaultCreator
    {
        /// <inheritdoc/>
        public async Task<IDisposable> CreateVaultAsync(IFolder vaultFolder, IEnumerable<IDisposable> passkey, VaultOptions vaultOptions, CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();
            using var passkeySecret = AuthenticationHelpers.ParseSecretKey(passkey);
            var options = ParseOptions(vaultOptions);

            var superSecret = await creationRoutine
                .SetCredentials(passkeySecret)
                .SetOptions(options)
                .FinalizeAsync(cancellationToken);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.TryCreateFileAsync(Constants.Vault.VAULT_README_FILENAME, false, cancellationToken);
                if (readmeFile is not null)
                    await readmeFile.WriteAllTextAsync(Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return superSecret;
        }

        private static IDictionary<string, string?> ParseOptions(VaultOptions vaultOptions)
        {
            return new Dictionary<string, string?>()
            {
                { Associations.ASSOC_CONTENT_CIPHER_ID, vaultOptions.ContentCipherId },
                { Associations.ASSOC_FILENAME_CIPHER_ID, vaultOptions.FileNameCipherId },
                { Associations.ASSOC_AUTHENTICATION, vaultOptions.AuthenticationMethod },
                { Associations.ASSOC_ID, vaultOptions.Id }
            };
        }
    }
}
