using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.Routines.Operational;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public class VaultManagerService : IVaultManagerService
    {
        /// <inheritdoc/>
        public virtual async Task<IDisposable> CreateAsync(IFolder vaultFolder, IKeyUsage passkey, VaultOptions vaultOptions, CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();
            await creationRoutine.InitAsync(cancellationToken);
            creationRoutine.SetCredentials(passkey);
            creationRoutine.SetOptions(vaultOptions);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.CreateFileAsync(Sdk.Constants.Vault.VAULT_README_FILENAME, true, cancellationToken);
                await readmeFile.WriteAllTextAsync(Sdk.Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return await creationRoutine.FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IDisposable> UnlockAsync(IFolder vaultFolder, IKeyUsage passkey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();

            await unlockRoutine.InitAsync(cancellationToken);
            unlockRoutine.SetCredentials(passkey);
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
        public virtual async Task ModifyAuthenticationAsync(IFolder vaultFolder, IDisposable unlockContract, IKeyUsage newPasskey, VaultOptions vaultOptions, CancellationToken cancellationToken = default)
        {
            using var credentialsRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).ModifyCredentials();
            await credentialsRoutine.InitAsync(cancellationToken);
            credentialsRoutine.SetUnlockContract(unlockContract);
            credentialsRoutine.SetOptions(vaultOptions);
            credentialsRoutine.SetCredentials(newPasskey);

            using var result = await credentialsRoutine.FinalizeAsync(cancellationToken);
        }
    }
}
