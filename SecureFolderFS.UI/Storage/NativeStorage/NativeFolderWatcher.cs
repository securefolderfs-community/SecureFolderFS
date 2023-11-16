using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.MutableStorage;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Storage.NativeStorage
{
    /// <inheritdoc cref="IFolderWatcher"/>
    public sealed class NativeFolderWatcher : IFolderWatcher
    {
        private int _handlersCount;
        private FileSystemWatcher? _fileSystemWatcher;
        private NotifyCollectionChangedEventHandler? _collectionChanged;

        public IMutableFolder Folder { get; }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add
            {
                if (_fileSystemWatcher is not null)
                    _fileSystemWatcher.EnableRaisingEvents = true;

                _collectionChanged += value;
                _handlersCount++;
            }
            remove
            {
                if (_fileSystemWatcher is not null && _handlersCount <= 1)
                    _fileSystemWatcher.EnableRaisingEvents = false;

                _collectionChanged -= value;
                _handlersCount--;
            }
        }

        public NativeFolderWatcher(IMutableFolder folder)
        {
            Folder = folder;
            SetupWatcher();
        }

        private void SetupWatcher()
        {
            if (Folder is not ILocatableFolder locatableFolder)
                throw new InvalidOperationException("The watcher folder is not locatable.");

            _fileSystemWatcher = new(locatableFolder.Path);
            _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            _fileSystemWatcher.Created += FileSystemWatcher_Created;
            _fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            _fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            _collectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, e.FullPath, e.FullPath));
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            _collectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, e.FullPath));
            _collectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, e.FullPath));
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _collectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, e.FullPath));
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            _collectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, e.FullPath));
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_fileSystemWatcher is null)
                return;

            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
            _fileSystemWatcher.Created -= FileSystemWatcher_Created;
            _fileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;
            _fileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;
            _fileSystemWatcher.Dispose();
        }
    }
}
