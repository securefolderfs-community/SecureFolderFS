using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Represents a service to interact with vault-related data.
    /// </summary>
    public interface IVaultService // TODO: Move some of the methods to IVaultModel?
    {
        /// <summary>
        /// Gets the latest vault version format.
        /// </summary>
        int LatestVaultVersion { get; }

        /// <summary>
        /// Gets the name of the vault's content folder.
        /// </summary>
        string ContentFolderName { get; }

        /// <summary>
        /// Gets the <see cref="IAsyncValidator{T}"/> of type <see cref="IFolder"/> used to validate vaults.
        /// </summary>
        IAsyncValidator<IFolder> VaultValidator { get; }

        /// <summary>
        /// Determines whether provided <paramref name="name"/> is part of vault core configuration files.
        /// </summary>
        /// <param name="name">The file name to check.</param>
        /// <returns>Returns true if the file name is a part of vault configuration data; otherwise false.</returns>
        bool IsNameReserved(string? name);

        /// <summary>
        /// Gets all encoding options that are supported by SecureFolderFS.
        /// </summary>
        /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="string"/> that represents IDs of encodings.</returns>
        IEnumerable<string> GetEncodingOptions();

        /// <summary>
        /// Gets an instance of <see cref="VaultOptions"/> that contains information about the vault.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="VaultOptions"/> of the additional vault information.</returns>
        Task<VaultOptions> GetVaultOptionsAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the appropriate migrator for a vault.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IVaultMigratorModel"/> that is used to migrate a vault.</returns>
        Task<IVaultMigratorModel> GetMigratorAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);
    }
}
