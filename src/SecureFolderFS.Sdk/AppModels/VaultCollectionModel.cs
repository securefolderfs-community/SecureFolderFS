using CommunityToolkit.Mvvm.Messaging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Storage;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    [Inject<IVaultPersistenceService>, Inject<IStorageService>, Inject<IApplicationService>]
    public sealed partial class VaultCollectionModel : Collection<IVaultModel>, IVaultCollectionModel
    {
        private IVaultWidgets VaultWidgets => VaultPersistenceService.VaultWidgets;

        private IVaultConfigurations VaultConfigurations => VaultPersistenceService.VaultConfigurations;

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public VaultCollectionModel()
        {
            ServiceProvider = DI.Default;
        }

        /// <inheritdoc/>
        public void Move(int oldIndex, int newIndex)
        {
            if (VaultConfigurations.PersistedVaults is null)
                return;

            var item = base[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);

            // Also update the backing store
            // Note, that we don't need to update the order for VaultWidgets
            var itemConfiguration = VaultConfigurations.PersistedVaults[oldIndex];
            VaultConfigurations.PersistedVaults.RemoveAt(oldIndex);
            VaultConfigurations.PersistedVaults.Insert(newIndex, itemConfiguration);

            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Move, item, oldIndex, newIndex));
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(VaultConfigurations.InitAsync(cancellationToken), VaultWidgets.InitAsync(cancellationToken));

            // Clear previous vaults
            Items.Clear();

            VaultConfigurations.PersistedVaults ??= new List<VaultDataModel>();
            foreach (var item in VaultConfigurations.PersistedVaults)
            {
                if (item.PersistableId is null)
                    continue;

                try
                {
                    var folder = await StorageService.GetPersistedAsync<IFolder>(item.PersistableId, cancellationToken);
                    var vaultModel = new VaultModel(folder, item.DisplayName, item.LastAccessDate);

                    Items.Add(vaultModel);
                    CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, vaultModel));
                }
                catch (Exception ex)
                {
                    _ = ex;
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(VaultConfigurations.SaveAsync(cancellationToken), VaultWidgets.SaveAsync(cancellationToken));
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            VaultConfigurations.PersistedVaults?.Clear();
            VaultWidgets.Clear();

            base.ClearItems();
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, IVaultModel item)
        {
            // Update saved vaults
            VaultConfigurations.PersistedVaults ??= new List<VaultDataModel>();
            VaultConfigurations.PersistedVaults.Insert(index, new(item.Folder.GetPersistableId(), item.VaultName, item.LastAccessDate, new LocalStorageSourceDataModel()));

            // Add default widgets for vault
            VaultWidgets.SetForVault(item.Folder.Id, new List<WidgetDataModel>()
            {
                new(Constants.Widgets.HEALTH_WIDGET_ID),
                ApplicationService.IsDesktop
                    ? new(Constants.Widgets.GRAPHS_WIDGET_ID)
                    : new(Constants.Widgets.AGGREGATED_DATA_WIDGET_ID)
            });

            // Add to cache
            base.InsertItem(index, item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, index));
            WeakReferenceMessenger.Default.Send(new VaultAddedMessage(item));
        }

        /// <inheritdoc/>
        protected override async void RemoveItem(int index)
        {
            var removedItem = this[index];

            // Remove persisted
            VaultConfigurations.PersistedVaults?.RemoveAt(index);

            // Remove widget data for that vault
            VaultWidgets.SetForVault(removedItem.Folder.Id, null);

            // Remove bookmark
            if (removedItem.Folder is IBookmark bookmark)
                await bookmark.RemoveBookmarkAsync();

            // Remove from cache
            base.RemoveItem(index);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, removedItem, index));
            WeakReferenceMessenger.Default.Send(new VaultRemovedMessage(removedItem));
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, IVaultModel item)
        {
            if (VaultConfigurations.PersistedVaults is null)
                return;

            VaultConfigurations.PersistedVaults[index] = new(item.Folder.GetPersistableId(), item.VaultName, item.LastAccessDate, new LocalStorageSourceDataModel());

            var oldItem = this[index];
            base.SetItem(index, item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }
    }
}
