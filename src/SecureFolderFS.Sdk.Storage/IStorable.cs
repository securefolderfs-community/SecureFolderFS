namespace SecureFolderFS.Sdk.Storage
{
    /// <summary>
    /// Represents a base storage object on the file system.
    /// </summary>
    public interface IStorable
    {
        /// <summary>
        /// Gets the unique and consistent identifier for this file or folder.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the storage object, if any.
        /// </summary>
        string Name { get; }
    }
}
