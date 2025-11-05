using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.Routines.Operational;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public class VaultManagerService : IVaultManagerService
    {
        /// <inheritdoc/>
        public virtual async Task<IDisposable> CreateAsync(IFolder vaultFolder, IKey passkey, VaultOptions vaultOptions, CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);

            await creationRoutine.InitAsync(cancellationToken);
            creationRoutine.SetCredentials(passkeySecret);
            creationRoutine.SetOptions(vaultOptions);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.CreateFileAsync(Sdk.Constants.Vault.VAULT_README_FILENAME, true, cancellationToken);
                await readmeFile.WriteAllTextAsync(Sdk.Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return await creationRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IDisposable> UnlockAsync(IFolder vaultFolder, IKey passkey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);

            await unlockRoutine.InitAsync(cancellationToken);
            unlockRoutine.SetCredentials(passkeySecret);
            return await unlockRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IDisposable> RecoverAsync(IFolder vaultFolder, string encodedRecoveryKey, CancellationToken cancellationToken = default)
        {
            using var recoveryKey = KeyPair.CombineRecoveryKey(encodedRecoveryKey);
            
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
            using var recoveryRoutine = routines.RecoverVault();

            await recoveryRoutine.InitAsync(cancellationToken);
            recoveryRoutine.SetCredentials(recoveryKey);
            return await recoveryRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ModifyAuthenticationAsync(IFolder vaultFolder, IDisposable unlockContract, IKey newPasskey, VaultOptions vaultOptions, CancellationToken cancellationToken = default)
        {
            using var credentialsRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).ModifyCredentials();
            using var newPasskeySecret = VaultHelpers.ParsePasskeySecret(newPasskey);

            await credentialsRoutine.InitAsync(cancellationToken);
            credentialsRoutine.SetUnlockContract(unlockContract);
            credentialsRoutine.SetOptions(vaultOptions);
            credentialsRoutine.SetCredentials(newPasskeySecret);

            using var result = await credentialsRoutine.FinalizeAsync(cancellationToken);
        }
    }
}
