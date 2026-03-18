using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetModel"/>
    public sealed class WidgetModel : IWidgetModel
    {
        private readonly IPersistable _widgetsStore;

        /// <inheritdoc/>
        public string WidgetId { get; }

        /// <inheritdoc/>
        public WidgetDataModel DataModel { get; }

        public WidgetModel(string widgetId, IPersistable widgetsStore, WidgetDataModel widgetDataModel)
        {
            WidgetId = widgetId;
            DataModel = widgetDataModel;
            _widgetsStore = widgetsStore;
        }

        /// <inheritdoc/>
        public Task<string?> GetWidgetDataAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DataModel.WidgetsData);
        }

        /// <inheritdoc/>
        public Task<bool> SetWidgetDataAsync(string? value, CancellationToken cancellationToken = default)
        {
            DataModel.WidgetsData = value;
            return _widgetsStore.TrySaveAsync(cancellationToken);
        }
    }
}
