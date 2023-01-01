using SecureFolderFS.Sdk.Storage.StorageProperties;

namespace SecureFolderFS.Core.WebDav.Storage.StorageProperties
{
    /// <inheritdoc cref="IStorageProperty{T}"/>
    internal interface IDavProperty : IModifiableProperty<object>
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <remarks>
        /// The name of this storage property is determined by the underlying implementation.
        /// </remarks>
        string Name { get; }
    }
}
