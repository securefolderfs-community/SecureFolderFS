using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Vault;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.UI.AppModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.ServiceImplementation.Vault
{
    /// <inheritdoc cref="IVaultUnlocker"/>
    public sealed class VaultUnlocker : IVaultUnlocker
    {
        ///<inheritdoc/>
        public async Task<IVaultLifetimeModel> UnlockAsync(IFolder vaultFolder, IDisposable credentials, CancellationToken cancellationToken = default)
        {
            //if (credentials is not CredentialsCombo credentialsCombo)
            //    throw new ArgumentException("Credentials were not in a correct format.", nameof(credentials));

            //var routines = await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
            //using var unlockRoutine = routines.UnlockVault();
            //await unlockRoutine.InitAsync(cancellationToken);

            //var unlockContract = await unlockRoutine
            //    .SetCredentials(credentialsCombo.Password, credentialsCombo.Authentication)
            //    .FinalizeAsync(cancellationToken);

            //using var storageRoutine = routines.BuildStorage();
            //var mountable = await storageRoutine
            //    .SetUnlockContract(unlockContract)
            //    .SetStorageService(null)
            //    .CreateMountableAsync(new FileSystemOptions(), cancellationToken);

            //var virtualFileSystem = await mountable.MountAsync(new(), cancellationToken);
            //return new VaultLifetimeModel(virtualFileSystem, null, null);

            throw new NotImplementedException();
        }
    }
}
