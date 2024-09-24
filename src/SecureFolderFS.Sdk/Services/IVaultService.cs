using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
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
        /// Gets all file systems that are supported by SecureFolderFS.
        /// </summary>
        /// <remarks>
        /// Returned file systems that are available, may not be supported on this device. 
        /// Use <see cref="IFileSystemInfoModel.GetStatusAsync"/> to check if a given file system is supported.
        /// </remarks>
        /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="IFileSystemInfoModel"/> of available file systems.</returns>
        IEnumerable<IFileSystemInfoModel> GetFileSystems();

        /// <summary>
        /// Gets all content ciphers that are supported by SecureFolderFS.
        /// </summary>
        /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="string"/> that represents IDs of content ciphers.</returns>
        IEnumerable<string> GetContentCiphers();

        /// <summary>
        /// Gets all filename ciphers that are supported by SecureFolderFS.
        /// </summary>
        /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="string"/> that represents IDs  of filename ciphers.</returns>
        IEnumerable<string> GetFileNameCiphers();

        /// <summary>
        /// Gets an instance of <see cref="VaultOptions"/> that contains information about the vault.
        /// </summary>
        /// <param name="vaultFolder">The folder of the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="VaultOptions"/> of the additional vault information.</returns>
        Task<VaultOptions> GetVaultOptionsAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        // TODO: Needs docs

        IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
