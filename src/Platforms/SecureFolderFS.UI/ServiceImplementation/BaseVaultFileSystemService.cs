using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    public abstract class BaseVaultFileSystemService : IVaultFileSystemService
    {
        /// <inheritdoc/>
        public Task<IFileSystemInfo> GetLocalFileSystemAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IFileSystemInfo>(new LocalFileSystem());
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<ItemInstallationViewModel> GetFileSystemInstallationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
        
        /// <inheritdoc/>
        public abstract IAsyncEnumerable<IFileSystemInfo> GetFileSystemsAsync(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<BaseDataSourceWizardViewModel> GetSourcesAsync(IVaultCollectionModel vaultCollectionModel, NewVaultMode mode, CancellationToken cancellationToken = default);
    }
}
