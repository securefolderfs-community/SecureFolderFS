using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// A service to print specialized content.
    /// </summary>
    public interface IPrinterService
    {
        /// <summary>
        /// Determines whether the printing operation is supported by the system.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If the printing is supported, returns true; otherwise false.</returns>
        Task<bool> IsSupportedAsync();

        /// <summary>
        /// Prints a formatted document containing vault's recovery key.
        /// </summary>
        /// <param name="vaultName">The name of the vault.</param>
        /// <param name="vaultId">The unique ID of the vault.</param>
        /// <param name="recoveryKey">The recovery key secret to print.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task PrintRecoveryKeyAsync(string vaultName, string? vaultId, string? recoveryKey);
    }
}
