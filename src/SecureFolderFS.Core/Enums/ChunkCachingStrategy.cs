namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Describes how chunk cache should be handled.
    /// </summary>
    public enum ChunkCachingStrategy : uint
    {
        /// <summary>
        /// No caching is used.
        /// </summary>
        NoCache = 0,

        /// <summary>
        /// Simple in-memory cache of chunks.
        /// </summary>
        RandomAccessMemoryCache = 1
    }
}
