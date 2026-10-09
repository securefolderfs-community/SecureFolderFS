using System;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.DeviceLink.Results
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
        /// The type of the mobile device.
        /// </summary>
        public required string MobileDeviceType { get; init; }

        /// <summary>
        /// The channel binding secret established during pairing. Persisted on the desktop and folded
        /// into every authentication session's channel key. Domain-separated from the vault key
        /// contribution, so storing it at rest does not expose any vault key material.
        /// </summary>
        public required byte[] BindingSecret { get; init; }

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