using System.Collections.ObjectModel;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// A manager type to keep track of all opened virtual file systems.
    /// </summary>
    public sealed class FileSystemManager
    {
        /// <summary>
        /// Gets the universal instance of <see cref="FileSystemManager"/>.
        /// </summary>
        public static FileSystemManager Instance { get; } = new();

        /// <summary>
        /// Gets the collection of registered <see cref="IVFSRoot"/>s.
        /// </summary>
        public ObservableCollection<IVFSRoot> FileSystems { get; }

        private FileSystemManager()
        {
            FileSystems = new();
        }
    }
}
