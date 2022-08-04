using System.Collections.Generic;
using System.Threading;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Represents a service to interact with vault-related data.
    /// </summary>
    public interface IVaultService // TODO: Rename this service since it doesn't have any vault related members
    {
        /// <summary>
        /// Gets the <see cref="IAsyncValidator{T}"/> to validate vaults.
        /// </summary>
        IAsyncValidator<IFolder> GetVaultValidator();

        /// <summary>
        /// Gets all file systems that are supported by SecureFolderFS.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <remarks>
        /// Returned file systems that are available, may not be supported on this device. 
        /// Use <see cref="IFileSystemInfoModel.IsSupportedAsync(CancellationToken)"/> to check if a given file system is supported.
        /// </remarks>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFileSystemInfoModel"/> of available file systems.</returns>
        IAsyncEnumerable<IFileSystemInfoModel> GetFileSystemsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all content ciphers that are supported by SecureFolderFS.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFileSystemInfoModel"/> of content ciphers.</returns>
        IAsyncEnumerable<ICipherInfoModel> GetContentCiphersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all filename ciphers that are supported by SecureFolderFS.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFileSystemInfoModel"/> of filename ciphers.</returns>
        IAsyncEnumerable<ICipherInfoModel> GetFilenameCiphersAsync(CancellationToken cancellationToken = default);
    }
}
