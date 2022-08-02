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
    public interface IVaultService
    {
        /// <summary>
        /// Gets the <see cref="IAsyncValidator{T}"/> to validate vaults.
        /// </summary>
        IAsyncValidator<IFolder> GetVaultValidator();

        /// <summary>
        /// Gets all file systems that are supported by SecureFolderFS.
        /// </summary>
        /// <remarks>
        /// Returned file systems that are available, may not be supported by this machine. 
        /// Use <see cref="IFileSystemInfoModel.IsSupportedAsync(CancellationToken)"/> to check if a given file system is supported.
        /// </remarks>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IFileSystemInfoModel"/> of available file systems.</returns>
        IAsyncEnumerable<IFileSystemInfoModel> GetFileSystemsAsync();
    }
}
