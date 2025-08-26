using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Shared;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetsCollectionModel"/>
    [Inject<IVaultPersistenceService>]
    public sealed partial class WidgetsCollectionModel : IWidgetsCollectionModel
    {
        private readonly string _id;
        private readonly List<IWidgetModel> _widgets;

        private IVaultWidgets VaultWidgets => VaultPersistenceService.VaultWidgets;

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public WidgetsCollectionModel(string persistableId)
        {
            ServiceProvider = DI.Default;
            _id = persistableId;
            _widgets = new();
        }

        /// <inheritdoc/>
        public bool AddWidget(string widgetId)
        {
            var widgets = VaultWidgets.GetForVault(_id) ?? new List<WidgetDataModel>();
            var widgetData = new WidgetDataModel(widgetId);

            // Add the widget to widget list
            widgets.Add(widgetData);

            // Update widgets
            VaultWidgets.SetForVault(_id, widgets);

            // Add to cache
            var widgetModel = new WidgetModel(widgetId, VaultWidgets, widgetData);
            _widgets.Add(widgetModel);

            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, widgetModel));

            return true;
        }

        /// <inheritdoc/>
        public bool RemoveWidget(string widgetId)
        {
            // Get widgets
            var widgets = VaultWidgets.GetForVault(_id);

            var itemToRemove = widgets?.FirstOrDefault(x => x.WidgetId == widgetId);
            if (itemToRemove is null)
                return false;

            // Remove from cache
            var widgetToRemove = _widgets.FirstOrDefault(x => x.WidgetId == widgetId);
            if (widgetToRemove is not null)
                _widgets.Remove(widgetToRemove);

            // Remove persisted
            widgets!.Remove(itemToRemove);
            VaultWidgets.SetForVault(_id, widgets);

            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, itemToRemove));

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<IWidgetModel> GetWidgets()
        {
            return _widgets;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            // await VaultWidgets.InitAsync(cancellationToken);
            // VaultWidgets already loaded by VaultCollectionModel // TODO: Load here as well because we shouldn't rely on the implementation

            // Clear previous widgets
            _widgets.Clear();

            var widgets = VaultWidgets.GetForVault(_id);
            if (widgets is null)
                return Task.CompletedTask;

            foreach (var item in widgets)
                _widgets.Add(new WidgetModel(item.WidgetId, VaultWidgets, item));

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return VaultWidgets.SaveAsync(cancellationToken);
        }
    }
}
