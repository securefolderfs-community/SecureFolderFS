using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.UI.DataModels
{
    [Serializable]
    public sealed record DeviceLinkVaultDataModel : VaultChallengeDataModel
    {
        /// <summary>
        /// The Credential ID (CID) that binds this vault to a mobile credential.
        /// </summary>
        [JsonPropertyName("credentialId")]
        [DefaultValue(null)]
        public required string? CredentialId { get; init; }
        
        /// <summary>
        /// Gets or sets the unique identifier of the endpoint device.
        /// </summary>
        [JsonPropertyName("endpointDeviceId")]
        [DefaultValue(null)]
        public string? EndpointDeviceId { get; set; }
        
        /// <summary>
        /// The mobile credential's signing public key (Base64).
        /// Used to verify challenge signatures.
        /// </summary>
        [JsonPropertyName("publicSigningKey")]
        [DefaultValue(null)]
        public required byte[]? PublicSigningKey { get; init; }

        /// <summary>
        /// Unique pairing identifier shared between desktop and mobile.
        /// </summary>
        [JsonPropertyName("pairingId")]
        [DefaultValue(null)]
        public required string? PairingId { get; init; }
    }
}
