using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for credentials provision changed events.
    /// </summary>
    public sealed class CredentialsProvisionChangedEventArgs(IKeyBytes clearProvision, IKeyBytes signedProvision) : EventArgs
    {
        /// <summary>
        /// Gets the clear credentials provision representation.
        /// </summary>
        public IKeyBytes ClearProvision { get; } = clearProvision;

        /// <summary>
        /// Gets the signed credentials provision representation using the user-provided credentials.
        /// </summary>
        public IKeyBytes SignedProvision { get; } = signedProvision;
    }
}
