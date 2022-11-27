using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetModel"/>
    public sealed class LocalWidgetModel : IWidgetModel
    {
        private readonly ISettingsModel _widgetsStore;
        private readonly WidgetDataModel _widgetDataModel;

        /// <inheritdoc/>
        public string WidgetId { get; }

        public LocalWidgetModel(string widgetId, ISettingsModel widgetsStore, WidgetDataModel widgetDataModel)
        {
            WidgetId = widgetId;
            _widgetsStore = widgetsStore;
            _widgetDataModel = widgetDataModel;
        }

        /// <inheritdoc/>
        public Task<string?> GetDataAsync(string key, CancellationToken cancellationToken = default)
        {
            if (_widgetDataModel.WidgetData.TryGetValue(key, out var value))
                return Task.FromResult(value);

            return Task.FromResult<string?>(null);
        }

        /// <inheritdoc/>
        public Task<bool> SetDataAsync(string key, string? value, CancellationToken cancellationToken = default)
        {
            _widgetDataModel.WidgetData[key] = value;

            return _widgetsStore.SaveSettingsAsync(cancellationToken);
        }
    }
}
