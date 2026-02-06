using System;
using System.Text.Json.Serialization;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Uno.DataModels
{
    [Serializable]
    public sealed record VaultDeviceLinkDataModel : VaultCapabilityDataModel
    {
        /// <summary>
        /// Unique pairing identifier shared between desktop and mobile.
        /// </summary>
        [JsonPropertyName("pairingId")]
        public required string? PairingId { get; set; }
        
        /// <summary>
        /// The Credential ID (CID) that binds this vault to a mobile credential.
        /// </summary>
        [JsonPropertyName("credentialId")]
        public required string CredentialId { get; init; }

        /// <summary>
        /// The mobile device's unique identifier.
        /// </summary>
        [JsonPropertyName("mobileDeviceId")]
        public required string? MobileDeviceId { get; set; }

        /// <summary>
        /// Human-readable mobile device name.
        /// </summary>
        [JsonPropertyName("mobileDeviceName")]
        public required string? MobileDeviceName { get; set; }

        /// <summary>
        /// The mobile credential's signing public key (Base64).
        /// Used to verify challenge signatures.
        /// </summary>
        [JsonPropertyName("publicSigningKey")]
        public byte[]? PublicSigningKey { get; set; }

        /// <summary>
        /// When the pairing was established.
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Protocol version used during pairing.
        /// </summary>
        [JsonPropertyName("protocolVersion")]
        public int ProtocolVersion { get; init; } = 2;
    }
}
