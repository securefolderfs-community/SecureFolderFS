using OwlCore.Storage;
using SecureFolderFS.Storage.Pickers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that interacts with the system file explorer.
    /// </summary>
    public interface IFileExplorerService : IFilePicker, IFolderPicker
    {
        /// <summary>
        /// Tries to open the provided <paramref name="folder"/> in platform's default file explorer.
        /// </summary>
        /// <param name="folder">The folder to open the file explorer at.</param>
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
    }
}
