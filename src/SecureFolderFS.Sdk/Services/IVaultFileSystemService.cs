using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultFileSystemService
    {
        /// <summary>
        /// Gets the local representation of a file system.
        /// </summary>
        /// <returns>A new local file system instance.</returns>
        Task<IFileSystem> GetLocalFileSystemAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets all file systems that are supported by the app.
        /// </summary>
        /// <remarks>
        /// Returned file systems that are available, may not be supported on this device. 
        /// Use <see cref="IFileSystem.GetStatusAsync"/> to check if a given file system is supported.
        /// </remarks>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="IFileSystem"/> of available file systems.</returns>
        IAsyncEnumerable<IFileSystem> GetFileSystemsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the <see cref="HealthIssueViewModel"/> implementation for associated <see cref="IResult"/> from item validation.
        /// </summary>
        /// <param name="result">The result of validation.</param>
        /// <param name="storable">The affected storable.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If available, value is <see cref="HealthIssueViewModel"/>; otherwise false.</returns>
        Task<HealthIssueViewModel?> GetIssueViewModelAsync(IResult result, IStorable storable, CancellationToken cancellationToken = default);
    }
}
