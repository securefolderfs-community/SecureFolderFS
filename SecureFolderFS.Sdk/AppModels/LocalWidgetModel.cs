using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetModel"/>
    public sealed class LocalWidgetModel : IWidgetModel
    {
        private readonly IPersistable _widgetsStore;
        private readonly WidgetDataModel _widgetDataModel;

        /// <inheritdoc/>
        public string WidgetId { get; }

        public LocalWidgetModel(string widgetId, IPersistable widgetsStore, WidgetDataModel widgetDataModel)
        {
            WidgetId = widgetId;
            _widgetsStore = widgetsStore;
            _widgetDataModel = widgetDataModel;
        }

        /// <inheritdoc/>
        public Task<string?> GetWidgetDataAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_widgetDataModel.WidgetsData);
        }

        /// <inheritdoc/>
        public Task<bool> SetWidgetDataAsync(string? value, CancellationToken cancellationToken = default)
        {
            _widgetDataModel.WidgetsData = value;
            return _widgetsStore.SaveAsync(cancellationToken);
        }
    }
}
