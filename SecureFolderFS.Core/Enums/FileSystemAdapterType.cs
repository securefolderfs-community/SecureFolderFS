namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Defines types of virtual file system adapters that can be used by SecureFolderFS.
    /// </summary>
    public enum FileSystemAdapterType : uint
    {
        /// <summary>
        /// Dokany file system adapter.
        /// </summary>
        DokanAdapter = 0,

        /// <summary>
        /// WebDav server file system adapter.
        /// </summary>
        WebDavAdapter = 1,

        /// <summary>
        /// FUSE file system adapter.
        /// </summary>
        FuseAdapter = 2
    }
}
