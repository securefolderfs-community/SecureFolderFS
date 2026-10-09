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
        /// The type of the mobile device.
        /// </summary>
        [JsonPropertyName("mobileDeviceType")]
        public required string? MobileDeviceType { get; set; }

        /// <summary>
        /// The channel binding secret folded into every authentication session's channel key.
        /// Only a device holding the credential's HMAC key can reproduce it. It is domain-separated
        /// from the vault key contribution, so its presence at rest reveals no vault key material.
        /// </summary>
        [JsonPropertyName("bindingSecret")]
        public required byte[] BindingSecret { get; init; }

        /// <summary>
        /// SHA-256 hash of the vault key contribution returned by the mobile device.
        /// Used to verify authentication responses; the contribution itself is never persisted.
        /// </summary>
        [JsonPropertyName("keyVerifier")]
        public required byte[] KeyVerifier { get; init; }

        /// <summary>
        /// When the pairing was established.
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; init; } = DateTime.Now;

        /// <summary>
        /// Protocol version used during pairing.
        /// </summary>
        [JsonPropertyName("protocolVersion")]
        public int ProtocolVersion { get; init; } = Sdk.DeviceLink.Constants.PROTOCOL_VERSION;
    }
}
