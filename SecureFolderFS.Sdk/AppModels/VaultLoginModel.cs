using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.MutableStorage;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    internal sealed class VaultLoginModel : IVaultLoginModel
    {
        private readonly IAsyncValidator<IFolder> _vaultValidator;
        private IFolderWatcher? _folderWatcher;

        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        /// <inheritdoc/>
        public IVaultModel VaultModel { get; }

        /// <inheritdoc/>
        public event EventHandler<IResult>? VaultChangedEvent;

        public VaultLoginModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
            _vaultValidator = VaultService.GetVaultValidator();
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

        private async void FolderWatcher_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null)
                return;

            if (e.Action == NotifyCollectionChangedAction.Replace && e.OldItems is not null)
            {
                var oldItem = Path.GetFileName(e.OldItems.Cast<string>().FirstOrDefault());
                var newItem = Path.GetFileName(e.NewItems.Cast<string>().FirstOrDefault());

                if (!VaultService.IsKeyFileName(oldItem) && !VaultService.IsKeyFileName(newItem))
                    return;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = Path.GetFileName(e.NewItems.Cast<string>().FirstOrDefault());

                if (!VaultService.IsKeyFileName(item))
                    return;
            }

            var validationResult = await _vaultValidator.ValidateAsync(VaultModel.Folder);
            VaultChangedEvent?.Invoke(this, validationResult);
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
