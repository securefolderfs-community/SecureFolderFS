using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultStorageService"/>
    public abstract class BaseVaultStorageService : IVaultStorageService
    {

        /// <inheritdoc/>
        public virtual Task<IFolder> CreateLocalStorageAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            try
            {
                var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
                var storageRoutine = routines.BuildStorage();

                var statisticsModel = new ConsolidatedStatisticsModel();
                var storageRoot = await storageRoutine
                    .SetUnlockContract(unlockContract)
                    .CreateStorageRootAsync(new FileSystemOptions()
                    {
                        VolumeName = vaultModel.VaultName, // TODO: Format name to exclude illegal characters
                        FileSystemId = string.Empty,
                        HealthStatistics = statisticsModel,
                        FileSystemStatistics = statisticsModel
                    }, cancellationToken);

                return storageRoot;
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
