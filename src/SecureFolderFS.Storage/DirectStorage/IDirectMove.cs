using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.DirectStorage
{
    /// <summary>
    /// Provides direct move operation of storage objects.
    /// </summary>
    public interface IDirectMove : IModifiableFolder
    {
        /// <summary>
        /// Moves a storable item out of the provided folder, and into this folder. Returns the new item that resides in this folder.
        /// </summary>
        Task<IStorableChild> MoveFromAsync(IStorableChild itemToMove, IModifiableFolder source, bool overwrite = default, CancellationToken cancellationToken = default);
    }
}
