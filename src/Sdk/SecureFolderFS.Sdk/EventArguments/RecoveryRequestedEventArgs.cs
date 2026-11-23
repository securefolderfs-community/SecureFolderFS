using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for requesting the vault to be unlocked using a recovery key.
    /// </summary>
    /// <param name="recoveryKey">The recovery key provided by the user.</param>
    public sealed class RecoveryRequestedEventArgs(string recoveryKey) : EventArgs
    {
        /// <summary>
        /// Gets the recovery key that should be used to unlock the vault.
        /// </summary>
        public string RecoveryKey { get; } = recoveryKey;
    }
}
