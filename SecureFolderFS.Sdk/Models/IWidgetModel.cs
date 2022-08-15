using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model of an individual widget.
    /// </summary>
    public interface IWidgetModel
    {
        /// <summary>
        /// Gets the unique id associated with this widget.
        /// </summary>
        string WidgetId { get; }

        /// <summary>
        /// Gets the data associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to associate the data with.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns value that can be null, otherwise null.</returns>
        Task<object?> GetDataAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the data associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to associate the data with.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true, otherwise false.</returns>
        Task<bool> SetDataAsync(string key, object? value, CancellationToken cancellationToken = default);
    }
}
