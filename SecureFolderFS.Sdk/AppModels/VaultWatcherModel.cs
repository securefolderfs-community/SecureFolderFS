using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LockableStorage;
using SecureFolderFS.Sdk.Storage.MutableStorage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    internal sealed class VaultWatcherModel : IVaultWatcherModel
    {
        private IFolderWatcher? _folderWatcher;

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        /// <inheritdoc/>
        public IFolder VaultFolder { get; }

        /// <inheritdoc/>
        public event EventHandler<IResult>? VaultChangedEvent;

        public VaultWatcherModel(IFolder vaultFolder)
        {
            VaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (VaultFolder is not IMutableFolder mutableVaultFolder)
                return;

            if (_folderWatcher is not null)
                return;

            try
            {
                _folderWatcher = await mutableVaultFolder.GetFolderWatcherAsync(cancellationToken);
                _folderWatcher.CollectionChanged += FolderWatcher_CollectionChanged;
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        /// <inheritdoc/>
        public async Task<IDisposable?> LockFolderAsync(CancellationToken cancellationToken = default)
        {
            if (VaultFolder is ILockableStorable lockableStorable)
                return await lockableStorable.ObtainLockAsync(cancellationToken);

            return null;
        }

        private void FolderWatcher_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null)
                return;

            IResult? result = null;
            if (e.Action == NotifyCollectionChangedAction.Replace && e.OldItems is not null)
            {
                var oldItem = Path.GetFileName(e.OldItems.Cast<string>().FirstOrDefault());
                var newItem = Path.GetFileName(e.NewItems.Cast<string>().FirstOrDefault());

                // Determine whether any of the changed files were integral parts of the vault
                if (VaultService.IsNameReserved(oldItem) || VaultService.IsNameReserved(newItem))
                    result = new CommonResult(false);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = Path.GetFileName(e.NewItems.Cast<string>().FirstOrDefault());

                // Determine if the deleted file was an integral part of the vault
                if (VaultService.IsNameReserved(item))
                    result = new CommonResult(false);
            }

            // If unassigned, an unrelated file/folder has changed - the result should be true
            result ??= CommonResult.Success;
            VaultChangedEvent?.Invoke(this, result);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_folderWatcher is not null)
            {
                _folderWatcher.CollectionChanged -= FolderWatcher_CollectionChanged;
                _folderWatcher.Dispose();
            }
        }
    }
}
