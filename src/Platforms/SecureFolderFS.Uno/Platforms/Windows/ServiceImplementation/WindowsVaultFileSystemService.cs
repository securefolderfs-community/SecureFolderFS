using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.AppModels;
using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public sealed class WindowsVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new WindowsWebDavFileSystem();
            yield return new DokanyFileSystem();
        }

        /// <inheritdoc/>
        public override FileSystemOptions GetFileSystemOptions(IVaultModel vaultModel, string fileSystemId)
        {
            return fileSystemId switch
            {
                Core.WebDav.Constants.FileSystem.FS_ID => new WebDavOptions()
                {
                    VolumeName = vaultModel.VaultName, // TODO: Sanitize name
                    HealthStatistics = HealthStatistics(),
                    FileSystemStatistics = FileSystemStatistics()
                },

                Core.Dokany.Constants.FileSystem.FS_ID => new DokanyOptions()
                {
                    VolumeName = vaultModel.VaultName, // TODO: Sanitize name
                    HealthStatistics = HealthStatistics(),
                    FileSystemStatistics = FileSystemStatistics()
                },

                _ => base.GetFileSystemOptions(vaultModel, fileSystemId)
            };

            IHealthStatistics HealthStatistics() => new HealthStatistics(vaultModel.Folder);
            IFileSystemStatistics FileSystemStatistics() => new FileSystemStatistics();
        }
    }
}
