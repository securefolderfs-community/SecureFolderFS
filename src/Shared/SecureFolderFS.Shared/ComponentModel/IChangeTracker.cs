namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// Represents a component that tracks whether changes have occurred.
    /// </summary>
    public interface IChangeTracker
    {
        /// <summary>
        /// Gets a value indicating whether the tracked item has been modified.
        /// </summary>
        bool WasModified { get; }
    }
}
