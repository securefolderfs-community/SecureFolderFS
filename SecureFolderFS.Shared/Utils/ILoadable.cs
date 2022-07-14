namespace SecureFolderFS.Shared.Utils
{
    /// <summary>
    /// Represents an object which reports when its resources has been loaded.
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// Gets the value that indicates whether the object's resources has been loaded.
        /// </summary>
        bool IsLoaded { get; }
    }
}
