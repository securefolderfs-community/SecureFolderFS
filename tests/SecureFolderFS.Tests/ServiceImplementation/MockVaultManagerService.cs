using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    internal sealed class MockVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override Task<IVFSRoot> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            return Task.FromException<IVFSRoot>(new NotImplementedException());
        }
    }
}
