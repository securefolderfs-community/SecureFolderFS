using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that interacts with the system clipboard.
    /// </summary>
    public interface IClipboardService
    {
        /// <summary>
        /// Checks and requests permission to access clipboard.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is granted returns true, otherwise false.</returns>
        Task<bool> IsClipboardAvailableAsync();

        /// <summary>
        /// Sets the clipboard data.
        /// </summary>
        /// <param name="data">The data to upload to clipboard.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> SetClipboardDataAsync(IClipboardDataModel data);

        /// <summary>
        /// Gets the current clipboard data.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and access is granted returns <see cref="IClipboardDataModel"/>, otherwise null.</returns>
        Task<IClipboardDataModel?> RequestClipboardDataAsync();

        /// <summary>
        /// Gets the whole clipboard data.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and access is granted returns <see cref="IEnumerable{T}"/> of type <see cref="IClipboardDataModel"/>, otherwise null.</returns>
        Task<IEnumerable<IClipboardDataModel>?> RequestFullClipboardDataAsync();
    }
}
