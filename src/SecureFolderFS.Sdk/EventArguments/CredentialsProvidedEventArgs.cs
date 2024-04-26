using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for authentication provided events.
    /// </summary>
    public sealed class CredentialsProvidedEventArgs(IKey authentication) : EventArgs
    {
        /// <summary>
        /// Gets the authentication that was provided.
        /// </summary>
        public IKey Authentication { get; } = authentication;
    }
}
