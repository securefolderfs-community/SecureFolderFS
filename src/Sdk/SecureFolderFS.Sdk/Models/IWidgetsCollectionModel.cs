using SecureFolderFS.Shared.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

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
        /// Updates the order of widgets based on the provided list of widgets.
        /// </summary>
        /// <param name="orderedWidgets">The newly ordered list of widgets.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        void UpdateOrder(IList<IWidgetModel> orderedWidgets);

        /// <summary>
        /// Gets all persisted widgets.
        /// </summary>
        /// <returns>Returns an <see cref="IEnumerable{T}"/> of type <see cref="IWidgetModel"/> of all widgets for a given vault.</returns>
        IEnumerable<IWidgetModel> GetWidgets();
    }
}
