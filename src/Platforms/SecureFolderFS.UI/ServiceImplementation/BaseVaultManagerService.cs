using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.Storage;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public abstract class BaseVaultManagerService : IVaultManagerService
    {
        /// <inheritdoc/>
        public async Task<IDisposable> CreateAsync(IFolder vaultFolder, IKey passkey, VaultOptions vaultOptions,
            CancellationToken cancellationToken = default)
        {
            using var creationRoutine = (await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken)).CreateVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);
            var options = VaultHelpers.ParseOptions(vaultOptions);

            var superSecret = await creationRoutine
                .SetCredentials(passkeySecret)
                .SetOptions(options)
                .FinalizeAsync(cancellationToken);

            if (vaultFolder is IModifiableFolder modifiableFolder)
            {
                var readmeFile = await modifiableFolder.CreateFileAsync(Constants.Vault.VAULT_README_FILENAME, false, cancellationToken);
                if (readmeFile is not null)
                    await readmeFile.WriteAllTextAsync(Constants.Vault.VAULT_README_MESSAGE, Encoding.UTF8, cancellationToken);
            }

            return superSecret;
        }

        /// <inheritdoc/>
        public async Task<IDisposable> UnlockAsync(IFolder vaultFolder, IKey passkey, CancellationToken cancellationToken = default)
        {
            var routines = await VaultRoutines.CreateRoutinesAsync(vaultFolder, StreamSerializer.Instance, cancellationToken);
            using var unlockRoutine = routines.UnlockVault();
            using var passkeySecret = VaultHelpers.ParsePasskeySecret(passkey);

            await unlockRoutine.InitAsync(cancellationToken);
            return await unlockRoutine
                .SetCredentials(passkeySecret)
                .FinalizeAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder> CreateLocalStorageAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            try
            {
                var contentFolder = await vaultModel.Folder.GetFolderByNameAsync(Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
                var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
                var statisticsModel = new ConsolidatedStatisticsModel();
                var (directoryIdCache, pathConverter, streamsAccess) = routines.BuildStorage()
                    .SetUnlockContract(unlockContract)
                    .CreateStorageComponents(contentFolder, new()
                {
                    VolumeName = vaultModel.VaultName, // TODO: Format name to exclude illegal characters
                    FileSystemId = string.Empty,
                    HealthStatistics = statisticsModel,
                    FileSystemStatistics = statisticsModel
                });

                return new CryptoFolder(contentFolder, streamsAccess, pathConverter, directoryIdCache);
            }
            catch (Exception ex)
            {
                // Make sure to dispose the unlock contract when failed
                unlockContract.Dispose();

                _ = ex;
                throw;
            }
        }

        /// <inheritdoc/>
        public abstract Task<IFolder> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken);
    }
}
