using System;
using System.Text.Json.Serialization;
using SecureFolderFS.Core.DataModels;

namespace SecureFolderFS.Uno.DataModels
{
    [Serializable]
    public sealed record VaultDeviceLinkDataModel : VaultChallengeDataModel
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
        /// The expected HMAC result from mobile (Base64).
        /// Used to verify the mobile device has the correct HMAC key.
        /// </summary>
        [JsonPropertyName("expectedHmac")]
        public required byte[] ExpectedHmac { get; init; }

        /// <summary>
        /// When the pairing was established.
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Protocol version used during pairing.
        /// </summary>
        [JsonPropertyName("protocolVersion")]
        public int ProtocolVersion { get; init; } = 4;
    }
}
