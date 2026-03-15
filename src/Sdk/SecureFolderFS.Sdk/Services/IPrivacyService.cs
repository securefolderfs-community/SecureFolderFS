using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service that provides privacy-related functionality.
    /// </summary>
    public interface IPrivacyService
    {
        /// <summary>
        /// Clears all traces of file system usage, including caches and recent access history.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Returns true if traces were successfully cleared.</returns>
        Task<bool> ClearTracesAsync(CancellationToken cancellationToken = default);
    }
}

