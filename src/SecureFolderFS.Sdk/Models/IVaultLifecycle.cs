using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using System;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model of an unlocked vault.
    /// </summary>
    public interface IVaultLifecycle : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets the unlocked root folder of the vault.
        /// </summary>
        IFolder RootFolder { get; }

        /// <summary>
        /// Gets the <see cref="IVaultStatisticsModel"/> which reports file system operations of the unlocked vault.
        /// </summary>
        IVaultStatisticsModel VaultStatisticsModel { get; }

        /// <summary>
        /// Gets the model that contains additional information about the vault.
        /// </summary>
        VaultOptions VaultOptions { get; }
    }
}
