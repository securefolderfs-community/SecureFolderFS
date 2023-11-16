using SecureFolderFS.Sdk.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Storage.ExtendableStorage
{
    /// <summary>
    /// Extends existing <see cref="IStorable"/> interface with additional properties.
    /// </summary>
    public interface IStorableExtended : IStorable
    {
        /// <summary>
        /// Gets an instance of <see cref="IBasicProperties"/> associated with this storage object.
        /// </summary>
        IBasicProperties Properties { get; }
    }
}
