using Android.Content;
using Android.Provider;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.Specialized;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    public sealed class RootCollection : IDisposable
    {
        public static RootCollection? Instance { get; private set; }

        private readonly Context _context;
        private readonly List<SafRoot> _roots;
        private readonly object _rootsLock = new();

        /// <remarks>
        /// The returned collection is a point-in-time snapshot. Roots are added and removed
        /// on vault lock/unlock while provider binder threads enumerate them concurrently.
        /// </remarks>
        public IReadOnlyList<SafRoot> Roots
        {
            get
            {
                lock (_rootsLock)
                    return _roots.ToArray();
            }
        }

        public RootCollection(Context context)
        {
            _context = context;
            _roots = new();
            Instance = this;

            FileSystemManager.Instance.FileSystems.CollectionChanged += FileSystemManager_CollectionChanged;
        }

        public SafRoot? GetSafRootForRootId(string rootId)
        {
            lock (_rootsLock)
                return _roots.FirstOrDefault(x => x.RootId == rootId);
        }

        private void FileSystemManager_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems?[0] is not IVfsRoot storageRoot)
                        return;

                    // Add to available roots
                    lock (_rootsLock)
                        _roots.Add(new(storageRoot, Guid.NewGuid().ToString()));

                    NotifySafChange();
                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems?[0] is not IVfsRoot storageRoot)
                        return;

                    // Remove from available roots
                    lock (_rootsLock)
                        _roots.RemoveMatch(x => x.StorageRoot == storageRoot);

                    NotifySafChange();
                    break;
                }
            }
            return;

            void NotifySafChange()
            {
                var rootsUri = DocumentsContract.BuildRootsUri(Constants.Android.FileSystem.AUTHORITY);
                if (rootsUri is not null)
                    _context.ContentResolver?.NotifyChange(rootsUri, null);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            FileSystemManager.Instance.FileSystems.CollectionChanged -= FileSystemManager_CollectionChanged;
        }
    }
}
