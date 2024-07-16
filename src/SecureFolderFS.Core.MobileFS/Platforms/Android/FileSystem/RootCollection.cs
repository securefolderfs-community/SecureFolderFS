using Android.App;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.Specialized;
using Android.Content;
using Android.Provider;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    internal sealed class RootCollection : IDisposable
    {
        private readonly Context _context;

        public List<SafRoot> Roots { get; }

        public RootCollection(Context context)
        {
            _context = context;
            Roots = new();

            FileSystemManager.Instance.CollectionChanged += FileSystemManager_CollectionChanged;
        }

        public SafRoot? GetRootForRid(string rid)
        {
            return Roots.FirstOrDefault(x => x.Rid == rid);
        }

        private void FileSystemManager_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems?[0] is not IVFSRoot storageRoot)
                        return;

                    // Add to available roots
                    Roots.Add(new(storageRoot, Guid.NewGuid().ToString()));
                    NotifySafChange();

                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems?[0] is not IVFSRoot storageRoot)
                        return;

                    // Remove from available roots
                    Roots.RemoveMatch(x => x.StorageRoot == storageRoot);
                    NotifySafChange();

                    break;
                }
            }
            return;

            void NotifySafChange()
            {
                // TODO: Make the authority change resistant to avoid unexpected errors
                // Authority taken from ContentProviderAttribute on FileSystemProvider
                var rootsUri = DocumentsContract.BuildRootsUri("com.securefolderfs.securefolderfs.provider");
                if (rootsUri is not null)
                    _context.ContentResolver?.NotifyChange(rootsUri, null);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            FileSystemManager.Instance.CollectionChanged -= FileSystemManager_CollectionChanged;
        }
    }
}
