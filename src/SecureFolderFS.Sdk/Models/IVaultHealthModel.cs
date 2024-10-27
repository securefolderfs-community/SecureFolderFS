using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Scanners;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    public interface IVaultHealthModel : IDisposable
    {
        /// <summary>
        /// Gets the ciphertext <see cref="IFolderScanner{T}"/> used to scan the vault and its contents.
        /// </summary>
        IFolderScanner<IStorableChild> FolderScanner { get; }

        /// <summary>
        /// Occurs when an issue is found within the file system structure.
        /// </summary>
        event EventHandler<IStorableChild>? IssueFound;

        /// <summary>
        /// Starts the scanning of the file system structure and reports errors through <see cref="IssueFound"/> event.
        /// </summary>
        /// <param name="progressModel">The model to report current progress to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ScanAsync(ProgressModel progressModel, CancellationToken cancellationToken = default);
    }
}
