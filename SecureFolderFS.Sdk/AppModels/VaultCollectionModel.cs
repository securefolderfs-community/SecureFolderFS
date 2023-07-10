using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    [Inject<IVaultPersistenceService>, Inject<IStorageService>]
    public sealed partial class VaultCollectionModel : Collection<IVaultModel>, IVaultCollectionModel
    {
        private IVaultWidgets VaultWidgets => VaultPersistenceService.VaultWidgets;

        private IVaultConfigurations VaultConfigurations => VaultPersistenceService.VaultConfigurations;

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public VaultCollectionModel()
        {
            ServiceProvider = Ioc.Default;
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            if (VaultConfigurations.SavedVaults is not null)
                VaultConfigurations.SavedVaults.Clear();

            VaultWidgets.Clear();

            base.ClearItems();
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, IVaultModel item)
        {
            // Update saved vaults
            VaultConfigurations.SavedVaults ??= new List<VaultDataModel>();
            VaultConfigurations.SavedVaults.Insert(index, new(item.Folder.Id, item.VaultName, item.LastAccessDate));

            // Update widgets
            VaultWidgets.SetForVault(item.Folder.Id, new List<WidgetDataModel>()
            {
                new(Constants.Widgets.HEALTH_WIDGET_ID),
                new(Constants.Widgets.GRAPHS_WIDGET_ID)
            });

            // Add to cache
            base.InsertItem(index, item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, index));
            WeakReferenceMessenger.Default.Send(new AddVaultMessage(item));
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var removedItem = this[index];

            // Remove persisted
            if (VaultConfigurations.SavedVaults is not null)
                VaultConfigurations.SavedVaults.RemoveAt(index);

            // Remove widgets data for that vault
            VaultWidgets.SetForVault(removedItem.Folder.Id, null);

            // Remove from cache
            base.RemoveItem(index);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, removedItem, index));
            WeakReferenceMessenger.Default.Send(new RemoveVaultMessage(removedItem));
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, IVaultModel item)
        {
            if (VaultConfigurations.SavedVaults is null)
                return;

            VaultConfigurations.SavedVaults[index] = new(item.Folder.Id, item.VaultName, item.LastAccessDate);

            var oldItem = this[index];
            base.SetItem(index, item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

        /// <inheritdoc/>
        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(VaultConfigurations.LoadAsync(cancellationToken), VaultWidgets.LoadAsync(cancellationToken));

            // Clear previous vaults
            Items.Clear();

            VaultConfigurations.SavedVaults ??= new List<VaultDataModel>();
            foreach (var item in VaultConfigurations.SavedVaults)
            {
                if (item.Id is null)
                    continue;

                var folder = await StorageService.TryGetFolderAsync(item.Id, cancellationToken);
                if (folder is null)
                    continue;

                var vaultModel = new VaultModel(folder, item.VaultName, item.LastAccessDate);
                Items.Add(vaultModel);
            }
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(VaultConfigurations.SaveAsync(cancellationToken), VaultWidgets.SaveAsync(cancellationToken));
        }
    }
}
