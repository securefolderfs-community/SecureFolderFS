namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Describes how DirectoryID cache should be handled.
    /// </summary>
    public enum DirectoryIdCachingStrategy : uint
    {
        /// <summary>
        /// No caching is used.
        /// </summary>
        NoCache = 0,

        /// <summary>
        /// Simple in-memory cache of DirectoryIDs.
        /// </summary>
        RandomAccessMemoryCache = 1
    }
}
