using System;

namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Represents a storage object property.
    /// </summary>
    public interface IStorageProperty<T>
    {
        /// <summary>
        /// Gets the value associated with the storage property.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// An event that's fired when value of the property is updated.
        /// </summary>
        event EventHandler<T>? ValueUpdated;
    }
}
