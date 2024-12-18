using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Represents a storage root of a virtual file system.
    /// </summary>
    public interface IVFSRoot : IWrapper<IFolder>, IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the fully-qualified name of the file system this storage root is a part of.
        /// </summary>
        string FileSystemName { get; }

        /// <summary>
        /// Gets the <see cref="FileSystemOptions"/> instance for this file system.
        /// </summary>
        FileSystemOptions Options { get; }
    }
}
