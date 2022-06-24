using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.Contracts;

namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Represents storage object property.
    /// </summary>
    public interface IStorageProperty
    {
        /// <summary>
        /// Gets the storage property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value associated with the storage property.
        /// </summary>
        object? Value { get; }

        /// <summary>
        /// Modifies the value of a storage property and sets it to <paramref name="newValue"/>.
        /// </summary>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="permissionContract">Used to request modify permission via a contract.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and the property was modified, returns true, otherwise false.</returns>
        Task<bool> ModifyAsync(object newValue, IStoragePermissionContract? permissionContract = null);
    }
}
