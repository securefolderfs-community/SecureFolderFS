namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Represents a file property.
    /// </summary>
    public interface IFileProperty
    {
        /// <summary>
        /// Gets the file property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value associated with the property.
        /// </summary>
        object Value { get; }
    }
}
