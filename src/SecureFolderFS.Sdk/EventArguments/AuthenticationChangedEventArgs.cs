using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for authentication changed events.
    /// </summary>
    public sealed class AuthenticationChangedEventArgs(IDisposable authentication) : EventArgs
    {
        /// <summary>
        /// Gets the authentication that was provided or affected.
        /// </summary>
        public IDisposable Authentication { get; } = authentication;
    }
}
