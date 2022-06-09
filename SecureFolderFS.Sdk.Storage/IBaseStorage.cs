using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Represents a base storage object on the file system.
    /// </summary>
    public interface IBaseStorage
    {
        /// <summary>
        /// Gets the path to the storage object.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The name of the storage object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the parent folder that contains this object.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, returns <see cref="IFolder"/>, otherwise null.</returns>
        Task<IFolder?> GetParentAsync();

        /// <summary>
        /// Deletes the associated storage object.
        /// </summary>
        /// <param name="permanently">Determines if the object should be sent to Recycle Bin.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task DeleteAsync(bool permanently);
    }
}
