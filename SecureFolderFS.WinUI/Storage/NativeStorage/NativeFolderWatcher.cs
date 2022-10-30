using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.MutableStorage;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.Storage.NativeStorage
{
    internal sealed class NativeFolderWatcher : IFolderWatcher
    {
        private FileSystemWatcher? _fileSystemWatcher;

        public IMutableFolder Folder { get; }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public NativeFolderWatcher(IMutableFolder folder)
        {
            Folder = folder;
            SetupWatcher();
        }

        private void SetupWatcher()
        {
            if (Folder is ILocatableFolder locatableFolder)
            {
                _fileSystemWatcher = new(locatableFolder.Path);
                _fileSystemWatcher.Created += FileSystemWatcher_Created;
                _fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
                _fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
            }
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, e.OldFullPath, e.FullPath));
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, e.FullPath));
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, e.FullPath));
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
            if (_fileSystemWatcher is not null)
            {
                _fileSystemWatcher.Created -= FileSystemWatcher_Created;
                _fileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;
                _fileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;
                _fileSystemWatcher.Dispose();
            }
        }
    }
}
