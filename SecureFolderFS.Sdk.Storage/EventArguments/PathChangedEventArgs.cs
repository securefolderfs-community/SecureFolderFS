namespace SecureFolderFS.Sdk.Storage.EventArguments
{
    /// <summary>
    /// Event arguments that contains the changed storage path.
    /// </summary>
    public sealed class PathChangedEventArgs
    {
        /// <summary>
        /// Gets the old path that has expired.
        /// </summary>
        public string? OldPath { get; }

        /// <summary>
        /// Gets the new path of the changed storage object.
        /// </summary>
        public string NewPath { get; }

        public PathChangedEventArgs(string newPath)
            : this(null, newPath)
        {
        }

        public PathChangedEventArgs(string? oldPath, string newPath)
        {
            OldPath = oldPath;
            NewPath = newPath;
        }
    }
}
