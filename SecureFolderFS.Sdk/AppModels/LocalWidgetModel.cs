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

        public LocalWidgetModel(ISettingsModel widgetsStore, WidgetDataModel widgetDataModel)
        {
            _widgetsStore = widgetsStore;
            _widgetDataModel = widgetDataModel;
        }

        /// <inheritdoc/>
        public Task<object?> GetDataAsync(string key, CancellationToken cancellationToken = default)
        {
            if (_widgetDataModel.WidgetData?.TryGetValue(key, out var value) ?? false)
                return Task.FromResult<object?>(value);

            return Task.FromResult<object?>(null);
        }

        /// <inheritdoc/>
        public Task<bool> SetDataAsync(string key, object? value, CancellationToken cancellationToken = default)
        {
            _widgetDataModel.WidgetData ??= new();
            _widgetDataModel.WidgetData[key] = value;

            return _widgetsStore.SaveSettingsAsync(cancellationToken);
        }
    }
}
