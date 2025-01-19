using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <inheritdoc cref="IVFSRoot"/>
    public abstract class VFSRoot : IVFSRoot, IWrapper<FileSystemSpecifics>
    {
        protected readonly IFolder storageRoot;
        protected readonly FileSystemSpecifics specifics;

        /// <inheritdoc/>
        IFolder IWrapper<IFolder>.Inner => storageRoot;

        /// <inheritdoc/>
        FileSystemSpecifics IWrapper<FileSystemSpecifics>.Inner => specifics;

        /// <inheritdoc/>
        public abstract string FileSystemName { get; }

        /// <inheritdoc/>
        public FileSystemOptions Options { get; }

        protected VFSRoot(IFolder storageRoot, FileSystemSpecifics specifics)
        {
            this.storageRoot = storageRoot;
            this.specifics = specifics;
            Options = specifics.FileSystemOptions;

            // Automatically add created root
            FileSystemManager.Instance.FileSystems.Add(this);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            specifics.Dispose();
        }

        /// <inheritdoc/>
        public virtual ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
