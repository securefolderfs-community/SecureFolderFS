namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Contains details about a vault.
    /// </summary>
    public sealed class VaultInfoModel
    {
        /// <summary>
        /// Gets the ID of the cipher that is used for content encryption.
        /// </summary>
        public string? ContentCipherId { get; init; }

        /// <summary>
        /// Gets the ID of the cipher that is used for filename encryption.
        /// </summary>
        public string? FileNameCipherId { get; init; }
    }
}
