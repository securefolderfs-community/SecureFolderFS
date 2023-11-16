using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.DirectStorage
{
    /// <summary>
    /// Provides direct move operation of storage objects.
    /// </summary>
    public interface IDirectMove : IModifiableFolder
    {
        /// <summary>
        /// Moves a storable item out of the provided folder, and into this folder. Returns the new item that resides in this folder.
        /// </summary>
        Task<INestedStorable> MoveFromAsync(INestedStorable itemToMove, IModifiableFolder source, bool overwrite = default, CancellationToken cancellationToken = default);
    }
}
