using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultFileSystemService
    {
        /// <summary>
        /// Gets the local representation of a file system.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A new local file system instance.</returns>
        Task<IFileSystem> GetLocalFileSystemAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets all file systems that are supported by the app.
        /// </summary>
        /// <remarks>
        /// Returned file systems that are available may not be supported on this device.
        /// Use <see cref="IFileSystem.GetStatusAsync"/> to check if a given file system is supported.
        /// </remarks>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFileSystem"/> of available file systems.</returns>
        IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the available file systems that can be installed on this device.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of type <see cref="ItemInstallationViewModel"/> representing the file system installations.</returns>
        IAsyncEnumerable<ItemInstallationViewModel> GetFileSystemInstallationsAsync(CancellationToken cancellationToken = default);

        IAsyncEnumerable<BaseDataSourceWizardViewModel> GetSourcesAsync(IVaultCollectionModel vaultCollectionModel, NewVaultMode mode, CancellationToken cancellationToken = default);
    }
}
