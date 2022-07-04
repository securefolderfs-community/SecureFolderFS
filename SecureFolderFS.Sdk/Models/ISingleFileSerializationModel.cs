using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a single file serialization model.
    /// </summary>
    public interface ISingleFileSerializationModel
    {
        /// <summary>
        /// Loads the database from <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The file to load the database from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>If database was successfully loaded, returns true otherwise false.</returns>
        Task<bool> LoadAsync(IFile file, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the database to <paramref name="file"/>.
        /// </summary>
        /// <param name="file">The database file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>If database was successfully saved, returns true otherwise false.</returns>
        Task<bool> SaveAsync(IFile file, CancellationToken cancellationToken = default);
    }
}
