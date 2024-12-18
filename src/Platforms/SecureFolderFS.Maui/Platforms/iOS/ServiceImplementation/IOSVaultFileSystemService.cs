using System.Runtime.CompilerServices;
using SecureFolderFS.Core.MobileFS.Platforms.iOS;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using IFileSystem = SecureFolderFS.Storage.VirtualFileSystem.IFileSystem;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class IOSVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new IOSFileSystem();
        }
    }
}
