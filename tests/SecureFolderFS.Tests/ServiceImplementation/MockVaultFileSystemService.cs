using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
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

        /// <inheritdoc/>
        public override IAsyncEnumerable<BaseDataSourceWizardViewModel> GetSourcesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
