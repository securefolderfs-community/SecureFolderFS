using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.ProjFS;
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
            yield return new WPFSFileSystem();
            yield return new DokanyFileSystem();
        }
    }
}
