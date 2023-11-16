﻿using SecureFolderFS.Sdk.Models;
using System;

namespace SecureFolderFS.Sdk.EventArguments
{
    /// <summary>
    /// Event arguments for vault unlocked events.
    /// </summary>
    public sealed class VaultUnlockedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IVaultLifecycle"/> of the unlocked vault.
        /// </summary>
        public IVaultLifecycle VaultLifecycle { get; }

        public VaultUnlockedEventArgs(IVaultLifecycle vaultLifecycle)
        {
            VaultLifecycle = vaultLifecycle;
        }
    }
}
