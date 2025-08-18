using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for password change events.
    /// </summary>
    public sealed class PasswordChangedEventArgs(bool isMatch) : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the two passwords match.
        /// </summary>
        public bool IsMatch { get; } = isMatch;
    }
}
