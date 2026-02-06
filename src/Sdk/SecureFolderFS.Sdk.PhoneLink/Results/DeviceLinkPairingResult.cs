using System;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.PhoneLink.Results
{
    public sealed class DeviceLinkPairingResult : Result<IKeyBytes>
    {
        /// <summary>
        /// The credential ID binding this pairing to a specific vault.
        /// </summary>
        public required string CredentialId { get; init; }

        /// <summary>
        /// The pairing ID for salt derivation.
        /// </summary>
        public required string PairingId { get; init; }

        /// <summary>
        /// The mobile device's unique identifier.
        /// </summary>
        public required string MobileDeviceId { get; init; }

        /// <summary>
        /// Human-readable mobile device name.
        /// </summary>
        public required string MobileDeviceName { get; init; }

        /// <summary>
        /// The mobile credential's signing public key.
        /// </summary>
        public required byte[] PublicSigningKey { get; init; }

        public DeviceLinkPairingResult(IKeyBytes value)
            : base(value)
        {
        }

        private DeviceLinkPairingResult(Exception? exception)
            : base(exception)
        {
        }

        private DeviceLinkPairingResult(IKeyBytes value, Exception exception)
            : base(value, exception)
        {
        }
    }
}