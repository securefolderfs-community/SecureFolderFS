namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// Describes the cryptographic parameters that were detected for a vault whose configuration is being rebuilt.
    /// </summary>
    /// <remarks>
    /// These are surfaced for confirmation before the rebuilt configuration is signed with the vault's own MAC key.
    /// </remarks>
    public sealed record class VaultRestorationParameters
    {
        /// <summary>
        /// Gets the ID of the detected content cipher.
        /// </summary>
        public required string ContentCipherId { get; init; }

        /// <summary>
        /// Gets the ID of the detected file name cipher.
        /// </summary>
        public required string FileNameCipherId { get; init; }

        /// <summary>
        /// Gets the ID of the detected file name encoding.
        /// </summary>
        public required string FileNameEncodingId { get; init; }

        /// <summary>
        /// Gets the detected threshold for shortening file names.
        /// </summary>
        public required int ShorteningThreshold { get; init; }

        /// <summary>
        /// Gets whether filename encryption was positively detected by an authenticated decryption.
        /// </summary>
        public required bool IsFileNameEncrypted { get; init; }
    }
}
