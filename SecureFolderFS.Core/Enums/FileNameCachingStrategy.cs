namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Describes how file name cache should be handled.
    /// </summary>
    public enum FileNameCachingStrategy
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
