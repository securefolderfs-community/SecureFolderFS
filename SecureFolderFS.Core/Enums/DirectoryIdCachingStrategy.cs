namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Describes how directory id cache should be handled.
    /// </summary>
    public enum DirectoryIdCachingStrategy : uint
    {
        /// <summary>
        /// No caching strategy is used.
        /// </summary>
        NoCache = 0,

        /// <summary>
        /// Simple cache based on dictionary of paths.
        /// </summary>
        RandomAccessMemoryCache = 1
    }
}
