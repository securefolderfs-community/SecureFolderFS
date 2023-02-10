using System.Threading.Tasks;

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
        /// Sets the current clipboard item to specified <paramref name="text"/>, if possible.
        /// </summary>
        /// <param name="text">The text to set to the clipboard.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetTextAsync(string text);
    }
}
