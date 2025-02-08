namespace SecureFolderFS.Shared.Models
{
    /// <summary>  
    /// Contains details about a vault.  
    /// </summary>  
    public sealed class VaultOptions
    {
        /// <summary>  
        /// Gets the information about the authentication method used for this vault.  
        /// </summary>  
        public required string[] AuthenticationMethod { get; init; }

        /// <summary>  
        /// Gets or sets whether to use recycle bin for the vault.
        /// </summary>  
        public bool IsRecycleBinEnabled { get; init; }

        /// <summary>  
        /// Gets the ID of the cipher that is used for content encryption.  
        /// </summary>  
        public string? ContentCipherId { get; init; }

        /// <summary>  
        /// Gets the ID of the cipher that is used for filename encryption.  
        /// </summary>  
        public string? FileNameCipherId { get; init; }

        /// <summary>  
        /// Gets the ID of the encoding method to use during filename encryption and decryption.  
        /// </summary>  
        public string? NameEncodingId { get; init; }

        /// <summary>  
        /// Gets the unique identifier of the vault.  
        /// </summary>  
        public string? VaultId { get; init; }

        /// <summary>  
        /// Gets the version format of the vault.  
        /// </summary>  
        public int Version { get; init; }
    }
}
