using OwlCore.Storage;
using System.Threading.Tasks;
using System.Threading;

namespace SecureFolderFS.Storage.Recyclable
{
    public interface IRecyclableFolder : IModifiableFolder
    {
        /// <summary>
        /// Deletes the provided storable item from this folder.
        /// </summary>
        /// <param name="item">The item to be removed from this folder.</param>
        /// <param name="sizeHint">The size in bytes of the item. If the value is below zero, the size will be calculated automatically.</param>
        /// <param name="deleteImmediately">Determines whether to delete the item immediately or not.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task DeleteAsync(IStorableChild item, long sizeHint, bool deleteImmediately = false, CancellationToken cancellationToken = default);
    }
}
