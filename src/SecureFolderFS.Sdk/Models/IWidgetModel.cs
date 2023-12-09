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
        /// Gets the data saved in this widget.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns value that can be null; otherwise null.</returns>
        Task<string?> GetWidgetDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the data for this widget.
        /// </summary>
        /// <param name="value">The data to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true; otherwise false.</returns>
        Task<bool> SetWidgetDataAsync(string? value, CancellationToken cancellationToken = default);
    }
}
