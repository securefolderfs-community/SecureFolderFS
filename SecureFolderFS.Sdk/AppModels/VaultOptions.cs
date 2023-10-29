namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Contains details about a vault.
    /// </summary>
    public sealed class VaultOptions
    {
        /// <summary>
        /// Gets the ID of the cipher that is used for content encryption.
        /// </summary>
        public required string ContentCipherId { get; init; }

        /// <summary>
        /// Gets the ID of the cipher that is used for filename encryption.
        /// </summary>
        public required string FileNameCipherId { get; init; }

        public required string Specialization { get; init; }

        public required string AuthenticationMethod { get; init; }
    }
}
