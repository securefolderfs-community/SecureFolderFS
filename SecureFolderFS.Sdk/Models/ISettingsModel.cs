using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages all actions related with storing app settings.
    /// </summary>
    public interface ISettingsModel
    {
        /// <summary>
        /// Determines whether settings are available.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Writes and persists all unsaved settings.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads all persisted settings and prepares them for use.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token of the action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default);
    }
}
