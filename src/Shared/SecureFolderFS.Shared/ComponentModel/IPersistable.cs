using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Allows for data to be saved to a persistence store.
    /// </summary>
    public interface ISavePersistence
    {
        /// <summary>
        /// Asynchronously saves data stored in memory.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation</returns>
        Task SaveAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Allows for data to be saved and loaded from a persistence store.
    /// </summary>
    public interface IPersistable : IAsyncInitialize, ISavePersistence
    {
    }
}
