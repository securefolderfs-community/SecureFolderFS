using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Represents an item in the recycle bin within the virtual file system.
    /// </summary>
    public interface IRecycleBinItem : IStorableChild, IWrapper<IStorableChild>, ICreatedAt, ISizeOf
    {
    }
}
