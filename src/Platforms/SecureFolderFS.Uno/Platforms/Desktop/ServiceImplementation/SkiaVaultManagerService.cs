using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.Routines.Operational;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.SkiaGtk.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    internal sealed class SkiaVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override async Task<IVFSRoot> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            try
            {
                var contentFolder = await vaultModel.Folder.GetFolderByNameAsync(Core.Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
                var routines = await VaultRoutines.CreateRoutinesAsync(vaultModel.Folder, StreamSerializer.Instance, cancellationToken);
                var statisticsModel = new ConsolidatedStatisticsModel();
                var storageRoutine = routines.BuildStorage();
                var options = new FileSystemOptions()
                {
                    VolumeName = vaultModel.VaultName, // TODO: Format name to exclude illegal characters
                    FileSystemId = await VaultHelpers.GetBestFileSystemAsync(cancellationToken),
                    HealthStatistics = statisticsModel,
                    FileSystemStatistics = statisticsModel
                };

                storageRoutine.SetUnlockContract(unlockContract);
                var (directoryIdCache, security, pathConverter, streamsAccess) = storageRoutine.CreateStorageComponents(contentFolder, options);
                var mountable = options.FileSystemId switch
                {
                    Core.Constants.FileSystemId.FS_FUSE => FuseMountable.CreateMountable(options, contentFolder, security, directoryIdCache, pathConverter, streamsAccess),
                    Core.Constants.FileSystemId.FS_WEBDAV => WebDavMountable.CreateMountable(options, contentFolder, security, directoryIdCache, pathConverter, streamsAccess),
                    _ => throw new ArgumentOutOfRangeException(nameof(options.FileSystemId))
                };

                return await mountable.MountAsync(options.FileSystemId switch
                {
                    Core.Constants.FileSystemId.FS_FUSE => new FuseMountOptions(),
                    Core.Constants.FileSystemId.FS_WEBDAV => new WebDavMountOptions() { Domain = "localhost", PreferredPort = 4949 },
                    _ => throw new ArgumentOutOfRangeException(nameof(options.FileSystemId))

                }, cancellationToken);
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
