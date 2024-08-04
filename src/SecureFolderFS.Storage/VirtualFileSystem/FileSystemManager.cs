using System.Collections.Generic;
using System.Collections.Specialized;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// A manager type to keep track of all opened virtual file systems.
    /// </summary>
    public sealed class FileSystemManager : INotifyCollectionChanged
    {
        private readonly List<IVFSRoot> _fileSystems;

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Gets the universal instance of <see cref="FileSystemManager"/>.
        /// </summary>
        public static FileSystemManager Instance { get; } = new();

        /// <summary>
        /// Gets the readonly list of registered <see cref="IVFSRoot"/>s.
        /// </summary>
        public IReadOnlyList<IVFSRoot> FileSystems => _fileSystems;

        private FileSystemManager()
        {
            _fileSystems = new();
        }

        /// <summary>
        /// Registers a new virtual file system root.
        /// </summary>
        /// <param name="vfsRoot">The <see cref="IVFSRoot"/> to register.</param>
        public void AddRoot(IVFSRoot vfsRoot)
        {
            _fileSystems.Add(vfsRoot);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, vfsRoot));
        }

        /// <summary>
        /// Removes a new virtual file system root from known file systems.
        /// </summary>
        /// <param name="vfsRoot">The <see cref="IVFSRoot"/> to remove.</param>
        public void RemoveRoot(IVFSRoot vfsRoot)
        {
            _fileSystems.Remove(vfsRoot);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, vfsRoot));
        }
    }
}
