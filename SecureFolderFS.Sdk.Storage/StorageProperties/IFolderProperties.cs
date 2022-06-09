using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Exposes access to folder properties.
    /// </summary>
    public interface IFolderProperties
    {
        /// <summary>
        /// Requests and returns folder properties.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If access is successful, returns <see cref="IEnumerable{T}"/> of type <see cref="IFolderProperty"/>, otherwise null.</returns>
        Task<IEnumerable<IFolderProperty>?> GetFolderPropertiesAsync();
    }
}
