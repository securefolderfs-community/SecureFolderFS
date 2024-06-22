using System.Threading;
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
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is granted returns true; otherwise false.</returns>
        Task<bool> IsSupportedAsync();

        /// <summary>
        /// Sets the current clipboard item to specified <paramref name="text"/>, if possible.
        /// </summary>
        /// <param name="text">The text to set to the clipboard.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetTextAsync(string text, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the text that is contained within the clipboard data.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="string"/> that represents the clipboard data.</returns>
        Task<string?> GetTextAsync(CancellationToken cancellationToken);
    }
}
