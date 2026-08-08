using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    /// <summary>
    /// Implemented by creation authentication view models that register the newly created
    /// vault key material with an App Platform server.
    /// </summary>
    public interface IAppPlatformVaultRegistration
    {
        /// <summary>
        /// Encrypts and uploads the vault key (DEK + MAC) to the App Platform server.
        /// </summary>
        /// <param name="vaultId">The unique identifier of the vault.</param>
        /// <param name="name">An optional human-friendly display name for the vault.</param>
        /// <param name="dekKey">The raw Data Encryption Key. Caller retains ownership.</param>
        /// <param name="macKey">The raw Message Authentication Code key. Caller retains ownership.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RegisterVaultAsync(string vaultId, string? name, IKeyUsage dekKey, IKeyUsage macKey, CancellationToken cancellationToken = default);
    }
}
