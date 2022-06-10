namespace SecureFolderFS.Sdk.Storage.StorageProperties
{
    /// <summary>
    /// Represents a folder property.
    /// </summary>
    public interface IFolderProperty
    {
        /// <summary>
        /// Gets the folder property name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the value associated with the property.
        /// </summary>
        object Value { get; }
    }
}
