using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Represents a service to interact with vault-related data.
    /// </summary>
    public interface IVaultService
    {
        /// <summary>
        /// Gets the default name for vault keystore file.
        /// </summary>
        string KeystoreFileName { get; } // TODO: Remove, Sdk shouldn't know about vault structure - that's handled by Core

        /// <summary>
        /// Gets the default name for vault configuration file.
        /// </summary>
        string ConfigurationFileName { get; } // TODO: Remove, Sdk shouldn't know about vault structure - that's handled by Core

        /// <summary>
        /// Determines whether provided <paramref name="name"/> is part of vault core configuration files.
        /// </summary>
        /// <param name="name">The file name to check.</param>
        /// <returns>Returns true if the file name is a part of vault configuration data, otherwise false.</returns>
        bool IsNameReserved(string? name);

        /// <summary>
        /// Gets the <see cref="IAsyncValidator{T}"/> of type <see cref="IFolder"/> used to validate vaults.
        /// </summary>
        IAsyncValidator<IFolder> GetVaultValidator();

        /// <summary>
        /// Gets all file systems that are supported by SecureFolderFS.
        /// </summary>
        /// <remarks>
        /// Returned file systems that are available, may not be supported on this device. 
        /// Use <see cref="IFileSystemInfoModel.IsSupportedAsync(CancellationToken)"/> to check if a given file system is supported.
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
    }
}
