namespace SecureFolderFS.Storage.StorageProperties
{
    /// <summary>
    /// Indicates that the storage item exposes a size property.
    /// </summary>
    public interface ISizeOf
    {
        /// <summary>
        /// Gets the size property for this storage item.
        /// </summary>
        ISizeOfProperty SizeOf { get; }
    }
}
