using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
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

        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        /// <inheritdoc/>
        public IVaultModel VaultModel { get; }

        /// <inheritdoc/>
        public event EventHandler<IResult>? VaultChangedEvent;

        public VaultWatcherModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }

        /// <inheritdoc/>
        public async Task<bool> WatchForChangesAsync(CancellationToken cancellationToken = default)
        {
            if (VaultModel.Folder is not IMutableFolder mutableVaultFolder)
                return false;

            if (_folderWatcher is not null)
                return true;

            try
            {
                _folderWatcher = await mutableVaultFolder.GetFolderWatcherAsync(cancellationToken);
                _folderWatcher.CollectionChanged += FolderWatcher_CollectionChanged;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public Task<IDisposable?> LockFolderAsync(CancellationToken cancellationToken = default)
        {
            return FileSystemService.ObtainLockAsync(VaultModel.Folder, cancellationToken);
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
                if (VaultService.IsFileNameReserved(oldItem) || VaultService.IsFileNameReserved(newItem))
                    result = new CommonResult(false);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = Path.GetFileName(e.NewItems.Cast<string>().FirstOrDefault());

                // Determine if the deleted file was an integral part of the vault
                if (VaultService.IsFileNameReserved(item))
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
