using System;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.DeviceLink.Results
{
    public sealed class DeviceLinkAuthenticationResult : Result<IKeyBytes>
    {
        /// <summary>
        /// The credential ID that was used for authentication.
        /// </summary>
        public required string CredentialId { get; init; }

        public DeviceLinkAuthenticationResult(IKeyBytes value)
            : base(value)
        {
        }

        private DeviceLinkAuthenticationResult(Exception? exception)
            : base(exception)
        {
        }

        private DeviceLinkAuthenticationResult(IKeyBytes value, Exception exception)
            : base(value, exception)
        {
        }
    }
}
