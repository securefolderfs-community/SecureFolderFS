using OwlCore.Storage;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault unlocked events.
    /// </summary>
    public sealed class VaultUnlockedEventArgs(IDisposable unlockContract, IFolder vaultFolder) : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IDisposable"/> that represents the contract of the unlocked vault.
        /// </summary>
        public IDisposable UnlockContract { get; } = unlockContract;

        /// <summary>
        /// Gets the <see cref="IFolder"/> of the unlocked vault.
        /// </summary>
        public IFolder VaultFolder { get; } = vaultFolder;
    }
}
