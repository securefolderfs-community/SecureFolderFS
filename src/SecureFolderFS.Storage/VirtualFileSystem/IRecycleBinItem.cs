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
    }
}
