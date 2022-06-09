using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Exposes access to file properties.
    /// </summary>
    public interface IFileProperties
    {
        /// <summary>
        /// Requests and returns file properties.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is successful, returns <see cref="IEnumerable{T}"/> of type <see cref="IFileProperty"/>, otherwise null.</returns>
        Task<IEnumerable<IFileProperty>?> GetFilePropertiesAsync();
    }
}
