namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Describes how chunk cache should be handled.
    /// </summary>
    public enum ChunkCachingStrategy : uint
    {
        /// <summary>
        /// No caching strategy is used.
        /// </summary>
        NoCache = 0,

        /// <summary>
        /// Simple cache based on dictionary of chunks.
        /// </summary>
        RandomAccessMemoryCache = 1,

        /// <summary>
        /// In-memory based cache of chunks.
        /// </summary>
        MemoryCache = 2
    }
}
