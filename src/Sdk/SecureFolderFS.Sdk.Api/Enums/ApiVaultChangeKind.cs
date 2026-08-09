namespace SecureFolderFS.Sdk.Api.Enums
{
    /// <summary>
    /// Describes the kind of change that occurred to a vault.
    /// </summary>
    public enum ApiVaultChangeKind
    {
        /// <summary>
        /// A vault became visible to integrations.
        /// </summary>
        Added,

        /// <summary>
        /// A vault stopped being visible to integrations, whether removed or hidden.
        /// </summary>
        Removed,

        /// <summary>
        /// A vault's display name changed.
        /// </summary>
        Renamed,

        /// <summary>
        /// A vault was mounted.
        /// </summary>
        Unlocked,

        /// <summary>
        /// A vault was unmounted.
        /// </summary>
        Locked
    }
}
