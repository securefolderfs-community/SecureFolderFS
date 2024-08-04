using OwlCore.Storage.System.IO;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    internal sealed class IOSVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override Task<IVFSRoot> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            // TODO: Add implementation for ios FS

            var statisticsModel = new ConsolidatedStatisticsModel();
            var disposable = new AggregatedDisposable([]);
            var root = new SystemFolder(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory);
            return Task.FromResult<IVFSRoot>(new LocalVFSRoot(disposable, root, new()
            {
                VolumeName = vaultModel.VaultName,
                FileSystemId = string.Empty,
                FileSystemStatistics = statisticsModel,
                HealthStatistics = statisticsModel
            }));
        }
    }
}
