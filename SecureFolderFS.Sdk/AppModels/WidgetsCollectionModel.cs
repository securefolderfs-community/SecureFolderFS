using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetsCollectionModel"/>
    public sealed class WidgetsCollectionModel : IWidgetsCollectionModel
    {
        private readonly IFolder _vaultFolder;

        private IVaultWidgets VaultWidgets { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultWidgets;

        public WidgetsCollectionModel(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public async Task<bool> AddWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            var widgets = VaultWidgets.GetForVault(_vaultFolder.Id) ?? new List<WidgetDataModel>();

            widgets.Add(new(widgetId));
            VaultWidgets.SetForVault(_vaultFolder.Id, widgets);

            return await VaultWidgets.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            var widgets = VaultWidgets.GetForVault(_vaultFolder.Id);

            if (widgets is null)
                return true;

            if (!widgets.TryFirstOrDefault(x => x.WidgetId == widgetId, out var itemToRemove))
                return true;

            widgets.Remove(itemToRemove);
            VaultWidgets.SetForVault(_vaultFolder.Id, widgets);

            return await VaultWidgets.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public IEnumerable<IWidgetModel> GetWidgets()
        {
            var widgets = VaultWidgets.GetForVault(_vaultFolder.Id);

            if (widgets is null)
                yield break;

            foreach (var item in widgets)
            {
                yield return new LocalWidgetModel(item.WidgetId, VaultWidgets, item);
            }
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await VaultWidgets.LoadAsync(cancellationToken);
        }
    }
}
