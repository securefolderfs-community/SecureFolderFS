using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetsCollectionModel"/>
    public sealed class WidgetsCollectionModel : IWidgetsCollectionModel
    {
        private readonly IFolder _vaultFolder;
        private readonly List<IWidgetModel> _widgets;

        private IVaultWidgets VaultWidgets { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultWidgets;

        public WidgetsCollectionModel(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
            _widgets = new();
        }

        /// <inheritdoc/>
        public bool AddWidget(string widgetId)
        {
            var widgets = VaultWidgets.GetForVault(_vaultFolder.Id) ?? new List<WidgetDataModel>();
            var widgetData = new WidgetDataModel(widgetId);

            // Add the widget to widget list
            widgets.Add(widgetData);

            // Update widgets
            VaultWidgets.SetForVault(_vaultFolder.Id, widgets);

            // Add to cache
            _widgets.Add(new WidgetModel(widgetId, VaultWidgets, widgetData));

            return true;
        }

        /// <inheritdoc/>
        public bool RemoveWidget(string widgetId)
        {
            var widgets = VaultWidgets.GetForVault(_vaultFolder.Id);
            
            var itemToRemove = widgets?.FirstOrDefault(x => x.WidgetId == widgetId);
            if (itemToRemove is null)
                return false;

            // Remove from cache
            var widgetToRemove = _widgets.FirstOrDefault(x => x.WidgetId == widgetId);
            if (widgetToRemove is not null)
                _widgets.Remove(widgetToRemove);

            // Remove persisted
            widgets!.Remove(itemToRemove);
            VaultWidgets.SetForVault(_vaultFolder.Id, widgets);

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<IWidgetModel> GetWidgets()
        {
            return _widgets;
        }

        /// <inheritdoc/>
        public Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            // await VaultWidgets.LoadAsync(cancellationToken);
            // VaultWidgets already loaded by VaultCollectionModel // TODO: Load here as well because we shouldn't rely on the implementation

            // Clear previous widgets
            _widgets.Clear();

            var widgets = VaultWidgets.GetForVault(_vaultFolder.Id);
            if (widgets is null)
                return Task.FromResult(true);

            foreach (var item in widgets)
                _widgets.Add(new WidgetModel(item.WidgetId, VaultWidgets, item));

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            return VaultWidgets.SaveAsync(cancellationToken);
        }
    }
}
