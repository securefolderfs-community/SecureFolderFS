using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.Services
{
    /// <summary>
    /// Represents a per-vault service used for the unlock routine.
    /// </summary>
    public interface IVaultUnlockingService : IDisposable
    {
        /// <summary>
        /// Sets the <see cref="IFolder"/> that represents the vault.
        /// </summary>
        /// <param name="folder">The folder of the vault to be set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that determines whether the vault folder was set successfully or not.</returns>
        Task<IResult> SetVaultFolderAsync(IFolder folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the <see cref="Stream"/> containing vault configuration.
        /// </summary>
        /// <param name="stream">The stream with vault configuration data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that determines whether the data from file was retrieved successfully or not.</returns>
        Task<IResult> SetConfigurationStreamAsync(Stream stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the <see cref="Stream"/> to keystore containing serialized vault keys.
        /// </summary>
        /// <param name="stream">The stream that contains the keystore.</param>
        /// <param name="serializer">The serializer used to deserialize the keystore.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that determines whether the serialization was successful or not.</returns>
        Task<IResult> SetKeystoreStreamAsync(Stream stream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the file system provider for this vault.
        /// </summary>
        /// <param name="fileSystemInfoModel">The file system provider to use for the vault instance.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult"/> that represents the action.</returns>
        Task<IResult> SetFileSystemAsync(IFileSystemInfoModel fileSystemInfoModel, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks and initializes the vault using the provided <paramref name="password"/>.
        /// </summary>
        /// <param name="password">The password to unlock the vault with.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IResult{T}"/> of <see cref="IUnlockedVaultModel"/> that represents the action.</returns>
        Task<IResult<IUnlockedVaultModel?>> UnlockAndStartAsync(IPassword password, CancellationToken cancellationToken = default);
    }
}
