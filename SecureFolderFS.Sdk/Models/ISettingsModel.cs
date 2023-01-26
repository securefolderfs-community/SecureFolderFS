using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Manages all actions related with storing app settings.
    /// </summary>
    public interface ISettingsModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Determines whether the settings store is available.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Loads all persisted settings and prepares them for use.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> LoadSettingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes and persists all settings.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful returns true, otherwise false.</returns>
        Task<bool> SaveSettingsAsync(CancellationToken cancellationToken = default);
    }
}
