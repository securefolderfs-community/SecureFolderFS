using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    internal sealed class MacOsVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new FuseFileSystem();
            yield return new MacOsWebDavFileSystem();
        }
    }
}
