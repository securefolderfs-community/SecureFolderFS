using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Storage.StorageProperties
{
    /// <summary>
    /// Provides access to properties of storage objects.
    /// </summary>
    public interface IStorableProperties : IStorable
    {
        /// <summary>
        /// Gets the <see cref="IBasicProperties"/> interface that allows access to all properties.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IBasicProperties"/> that exposes access to storage object properties.</returns>
        Task<IBasicProperties> GetPropertiesAsync();
    }
}
