using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultStorageService"/>
    public class VaultStorageService : IVaultStorageService
    {
        /// <inheritdoc/>
        public async Task<IFolder> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            try
            {
                var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
                var storageRoutine = routines.BuildStorage();
                var fileSystemId = await VaultHelpers.GetBestFileSystemAsync(cancellationToken);

                var statisticsModel = new ConsolidatedStatisticsModel();
                var mountable = await storageRoutine
                    .SetUnlockContract(unlockContract)
                    .CreateMountableAsync(new FileSystemOptions()
                    {
                        VolumeName = vaultModel.VaultName, // TODO: Format name to exclude illegal characters
                        FileSystemId = fileSystemId,
                        HealthStatistics = statisticsModel,
                        FileSystemStatistics = statisticsModel
                    }, cancellationToken);

                return await mountable.MountAsync(VaultHelpers.GetMountOptions(fileSystemId), cancellationToken);
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
        public async Task<IFolder> CreateLocalStorageAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
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
    }
}
