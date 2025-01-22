using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.Renamable
{
    /// <summary>
    /// Represents a folder that can rename items.
    /// </summary>
    public interface IRenamableFolder : IModifiableFolder
    {
        /// <summary>
        /// Renames the specified storable to a new name.
        /// </summary>
        /// <param name="storable">The storable to rename.</param>
        /// <param name="newName">The new name for the storable.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result contains the renamed storable.</returns>
        Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default);
    }
}
