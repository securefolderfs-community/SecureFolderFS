namespace SecureFolderFS.Backend.Enums
{
    public enum VaultHealthState
    {
        /// <summary>
        /// The vault is healthy and does not have any problems
        /// </summary>
        Healthy = 0,

        /// <summary>
        /// The vault needs attention (usually when the version is outdated)
        /// </summary>
        NeedsAttention = 1,

        /// <summary>
        /// Problems have been detected with vault's integrity
        /// </summary>
        Error = 2
    }
}
