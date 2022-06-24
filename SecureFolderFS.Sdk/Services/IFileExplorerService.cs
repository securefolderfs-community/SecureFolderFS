using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that interacts with the system file explorer.
    /// </summary>
    public interface IFileExplorerService
    {
        /// <summary>
        /// Opens the app folder.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenAppFolderAsync();

        /// <summary>
        /// Opens file explorer on provided <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to open.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenPathInFileExplorerAsync(string path);

        /// <summary>
        /// Awaits the user input and picks single file from the file explorer dialog.
        /// </summary>
        /// <param name="filter">The filter to apply when picking files.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and a file has been picked, returns <see cref="IFile"/>, otherwise null.</returns>
        Task<IFile?> PickSingleFileAsync(IEnumerable<string>? filter);

        /// <summary>
        /// Awaits the user input and picks single folder from the file explorer dialog.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and a folder has been picked, returns <see cref="IFolder"/>, otherwise null.</returns>
        Task<IFolder?> PickSingleFolderAsync();
    }
}
