using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Represents a model of an unlocked vault.
    /// </summary>
    public interface IUnlockedVaultModel
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
        /// Locks the vault, invalidates associated resources and restricts vault file system access.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task LockAsync();
    }
}
