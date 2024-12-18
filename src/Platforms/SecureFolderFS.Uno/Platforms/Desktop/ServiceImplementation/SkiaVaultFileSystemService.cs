using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class SkiaVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new SkiaWebDavFileSystem();
            yield return new FuseFileSystem();
        }

        /// <inheritdoc/>
        public override FileSystemOptions GetFileSystemOptions(IVaultModel vaultModel, string fileSystemId)
        {
            var statistics = new ConsolidatedStatisticsModel();
            return fileSystemId switch
            {
                Core.WebDav.Constants.FileSystem.FS_ID => new WebDavOptions()
                {
                    VolumeName = vaultModel.VaultName, // TODO: Sanitize name
                    HealthStatistics = statistics,
                    FileSystemStatistics = statistics
                },

                Core.FUSE.Constants.FileSystem.FS_ID => new FuseOptions()
                {
                    VolumeName = vaultModel.VaultName, // TODO: Sanitize name
                    HealthStatistics = statistics,
                    FileSystemStatistics = statistics
                },

                _ => base.GetFileSystemOptions(vaultModel, fileSystemId)
            };
        }
    }
}
