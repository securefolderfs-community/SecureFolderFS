namespace SecureFolderFS.Core.Enums
{
    /// <summary>
    /// Describes how file name cache should be handled.
    /// </summary>
    public enum FileNameCachingStrategy
    {
        /// <summary>
        /// No caching is used.
        /// </summary>
        NoCache = 0,

        /// <summary>
        /// Simple in-memory cache of file names.
        /// </summary>
        RandomAccessMemoryCache = 1
    }
}
