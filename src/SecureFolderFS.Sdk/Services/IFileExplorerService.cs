using OwlCore.Storage;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that interacts with the system file explorer.
    /// </summary>
    public interface IFileExplorerService
    {
        /// <summary>
        /// Tries to open the provided <paramref name="folder"/> in platform's default file explorer.
        /// </summary>
        /// <param name="folder">The folder to open file explorer in.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Awaits the user input and saves single file from the file explorer dialog.
        /// </summary>
        /// <param name="suggestedName">The name that will be suggested when saving the file.</param>
        /// <param name="dataStream">The data stream to use as the content of the saved file.</param>
        /// <param name="filter">The filter to apply when saving files.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Awaits the user input and picks single file from the file explorer dialog.
        /// </summary>
        /// <param name="filter">The filter to apply when picking files.</param>
        /// <param name="persist">Determines whether to persist access to the picked item or not.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and a file has been picked, returns <see cref="IFile"/>; otherwise null.</returns>
        Task<IFile?> PickFileAsync(IEnumerable<string>? filter, bool persist = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Awaits the user input and picks single folder from the file explorer dialog.
        /// </summary>
        /// <param name="persist">Determines whether to persist access to the picked item or not.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and a folder has been picked, returns <see cref="IFolder"/>; otherwise null.</returns>
        Task<IFolder?> PickFolderAsync(bool persist = true, CancellationToken cancellationToken = default);
    }
}
