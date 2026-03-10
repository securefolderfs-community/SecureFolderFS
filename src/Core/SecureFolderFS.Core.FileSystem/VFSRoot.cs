using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <inheritdoc cref="IVFSRoot"/>
    public abstract class VFSRoot : IVFSRoot, IWrapper<FileSystemSpecifics>
    {
        protected readonly FileSystemSpecifics specifics;

        /// <inheritdoc/>
        FileSystemSpecifics IWrapper<FileSystemSpecifics>.Inner => specifics;

        /// <inheritdoc/>
        public IFolder VirtualizedRoot { get; }

        /// <inheritdoc/>
        public IFolder PlaintextRoot { get; }

        /// <inheritdoc/>
        public abstract string FileSystemName { get; }

        /// <inheritdoc/>
        public VirtualFileSystemOptions Options { get; }

        protected VFSRoot(IFolder virtualizedRoot, IFolder plaintextRoot, FileSystemSpecifics specifics)
        {
            this.specifics = specifics;
            VirtualizedRoot = virtualizedRoot;
            PlaintextRoot = plaintextRoot;
            Options = specifics.Options;

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
