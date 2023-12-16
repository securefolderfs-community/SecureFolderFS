using SecureFolderFS.Shared.Utilities;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a context that holds widgets settings and layout of an individual vault.
    /// </summary>
    public interface IWidgetsCollectionModel : INotifyCollectionChanged, IPersistable // TODO: Inherit from ICollection
    {
        /// <summary>
        /// Adds a new widget model identified by <paramref name="widgetId"/>.
        /// </summary>
        /// <param name="widgetId">The id of a widget.</param>
        /// <returns>If successful, returns true; otherwise false.</returns>
        bool AddWidget(string widgetId);

        /// <summary>
        /// Removes persisted widget model identified by <paramref name="widgetId"/>.
        /// </summary>
        /// <param name="widgetId">The id of a widget.</param>
        /// <returns>If successful, returns true; otherwise false.</returns>
        bool RemoveWidget(string widgetId);

        /// <summary>
        /// Gets all persisted widgets.
        /// </summary>
        /// <returns>Returns an <see cref="IEnumerable{T}"/> of type <see cref="IWidgetModel"/> of all widgets for a given vault.</returns>
        IEnumerable<IWidgetModel> GetWidgets();
    }
}
