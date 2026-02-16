using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Represents an item in the recycle bin within the virtual file system.
    /// </summary>
    public interface IRecycleBinItem : IStorableChild, IWrapper<IStorableChild>
    {
        /// <summary>
        /// Gets the timestamp indicating when the item was deleted.
        /// </summary>
        DateTime DeletionTimestamp { get; }

        /// <summary>
        /// Gets the size of the item in bytes. If the size is unknown, the value is -1.
        /// </summary>
        long Size { get; }
    }
}
