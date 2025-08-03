using System.Collections.Generic;
using System.Threading;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultCredentialsService
    {
        /// <summary>
        /// Gets all encoding options that are supported by SecureFolderFS.
        /// </summary>
        /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="string"/> that represents IDs of encodings.</returns>
        IEnumerable<string> GetEncodingOptions();

        /// <summary>
        /// Gets all content ciphers that are supported by SecureFolderFS.
        /// </summary>
        /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="string"/> that represents IDs of content ciphers.</returns>
        IEnumerable<string> GetContentCiphers();

        /// <summary>
        /// Gets all filename ciphers that are supported by SecureFolderFS.
        /// </summary>
        /// <returns>Returns <see cref="IEnumerable{T}"/> of type <see cref="string"/> that represents IDs of filename ciphers.</returns>
        IEnumerable<string> GetFileNameCiphers();

        /// <summary>
        /// Asynchronously retrieves a collection of login authentication methods for a specified vault folder.
        /// </summary>
        /// <param name="vaultFolder">The folder representing the vault for which authentication methods should be retrieved.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="AuthenticationViewModel"/> representing available authentication methods.</returns>
        IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously retrieves authentication options for creating a vault.
        /// </summary>
        /// <param name="vaultFolder">The folder associated with the vault.</param>
        /// <param name="vaultId">The unique identifier of the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="AuthenticationViewModel"/> representing available authentication options.</returns>
        IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
