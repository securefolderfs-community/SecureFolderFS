namespace SecureFolderFS.Shared
{
    public static class SharedConfiguration
    {
        /// <summary>
        /// Enables memory-hardening features such as pinning and page locking.
        /// </summary>
        public static bool UseMemoryHardening { get; set; } = true;
    }
}
