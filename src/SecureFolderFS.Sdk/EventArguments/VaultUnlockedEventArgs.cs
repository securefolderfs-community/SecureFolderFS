using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault unlocked events.
    /// </summary>
    public sealed class VaultUnlockedEventArgs(IDisposable unlockContract, IVaultModel vaultModel) : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IDisposable"/> that represents the contract of the unlocked vault.
        /// </summary>
        public IDisposable UnlockContract { get; } = unlockContract;

        /// <summary>
        /// Gets the <see cref="IVaultModel"/> of the unlocked vault.
        /// </summary>
        public IVaultModel VaultModel { get; } = vaultModel;
    }
}
