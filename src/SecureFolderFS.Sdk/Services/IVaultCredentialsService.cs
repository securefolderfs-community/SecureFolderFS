using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultCredentialsService
    {
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

        // TODO: Needs docs

        IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
