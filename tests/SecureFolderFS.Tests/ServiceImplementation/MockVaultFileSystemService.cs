using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class MockVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
