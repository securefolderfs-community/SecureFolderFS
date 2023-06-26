namespace SecureFolderFS.Sdk.Storage.LocatableStorage
{
    /// <summary>
    /// Represents a storage object that resides within a folder structure.
    /// </summary>
    public interface ILocatableStorable : IStorable
    {
        /// <summary>
        /// Gets the path where the item resides.
        /// </summary>
        string Path { get; }
    }
}
