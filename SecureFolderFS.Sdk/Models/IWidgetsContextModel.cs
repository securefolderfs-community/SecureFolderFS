using SecureFolderFS.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a context that holds widgets settings and layout of an individual vault.
    /// </summary>
    public interface IWidgetsContextModel : IAsyncInitialize
    {
        /// <summary>
        /// Adds a new widget model identified by <paramref name="widgetId"/>.
        /// </summary>
        /// <param name="widgetId">The id of a widget.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true, otherwise false..</returns>
        Task<bool> AddWidgetAsync(string widgetId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes persisted widget model identified by <paramref name="widgetId"/>.
        /// </summary>
        /// <param name="widgetId">The id of a widget.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns true, otherwise false.</returns>
        Task<bool> RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all persisted widgets.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IWidgetModel"/> of items in the directory.</returns>
        IAsyncEnumerable<IWidgetModel> GetWidgetsAsync(CancellationToken cancellationToken = default);
    }
}
