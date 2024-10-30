using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.AppModels;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    public abstract class BaseVaultFileSystemService : IVaultFileSystemService
    {
        /// <inheritdoc/>
        public Task<IFileSystem> GetLocalFileSystemAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IFileSystem>(new LocalFileSystem());
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public virtual FileSystemOptions GetFileSystemOptions(IVaultModel vaultModel, string fileSystemId)
        {
            var healthStatistics = new HealthStatistics(vaultModel.Folder);
            var fileSystemStatistics = new FileSystemStatistics();

            return new FileSystemOptions()
            {
                VolumeName = vaultModel.VaultName, // TODO: Sanitize name
                HealthStatistics = healthStatistics,
                FileSystemStatistics = fileSystemStatistics
            };
        }
    }
}
