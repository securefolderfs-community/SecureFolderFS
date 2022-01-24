namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Defines types of virtual file system adapters for SecureFolderFS to use.
    /// </summary>
    public enum FileSystemAdapterType
    {
        /// <summary>
        /// Undefined state.
        /// </summary>
        None = 0,

        /// <summary>
        /// Dokan file system adapter.
        /// </summary>
        DokanAdapter = 1
    }
}
