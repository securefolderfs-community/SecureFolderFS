namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Describes how directory id cache should be handled.
    /// </summary>
    public enum DirectoryIdCachingStrategy : uint
    {
        /// <summary>
        /// No caching is used.
        /// </summary>
        NoCache = 0,

        /// <summary>
        /// Simple in-memory cache of directory IDs.
        /// </summary>
        RandomAccessMemoryCache = 1
    }
}
