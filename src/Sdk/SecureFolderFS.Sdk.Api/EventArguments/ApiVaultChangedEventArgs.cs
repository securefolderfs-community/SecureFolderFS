using System;
using SecureFolderFS.Sdk.Api.Enums;
using SecureFolderFS.Sdk.Api.Models;

namespace SecureFolderFS.Sdk.Api.EventArguments
{
    /// <summary>
    /// Event arguments for a vault change.
    /// </summary>
    public sealed class ApiVaultChangedEventArgs(ApiVaultChangeKind kind, string vaultId, VaultInfo? vault) : EventArgs
    {
        /// <summary>
        /// Gets the kind of change that occurred.
        /// </summary>
        public ApiVaultChangeKind Kind { get; } = kind;

        /// <summary>
        /// Gets the public identifier of the affected vault.
        /// </summary>
        public string VaultId { get; } = vaultId;

        /// <summary>
        /// Gets the vault's new state, or <see langword="null"/> when it was removed.
        /// </summary>
        public VaultInfo? Vault { get; } = vault;
    }
}
