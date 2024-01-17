using System;
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
        /// Prints a formatted document containing vault's master key.
        /// </summary>
        /// <param name="superSecret">The master key secret to print.</param>
        /// <param name="vaultName">The name of the vault.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task PrintMasterKeyAsync(IDisposable? superSecret, string vaultName);
    }
}
