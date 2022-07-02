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
    public interface IVaultUnlockingService
    {
        /// <summary>
        /// Sets the root <see cref="IFolder"/> of the vault.
        /// </summary>
        /// <param name="folder">The folder of the vault to be set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and the vault folder is valid, returns true otherwise false.</returns>
        Task<bool> SetVaultFolderAsync(IFolder folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the <see cref="Stream"/> to keystore containing serialized configuration.
        /// </summary>
        /// <param name="stream">The stream that contains the keystore.</param>
        /// <param name="serializer">The serializer used to deserialize the keystore.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If serialization is successful, returns true otherwise false.</returns>
        Task<bool> SetKeystoreStreamAsync(Stream stream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks the vault using the provided <paramref name="password"/>.
        /// </summary>
        /// <exception cref="IOException"></exception> // TODO: Add exceptions
        /// <param name="password">The password to unlock the vault with.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful, the value is <see cref="IUnlockedVaultModel"/>, otherwise null.</returns>
        Task<IUnlockedVaultModel?> UnlockAsync(IPassword password, CancellationToken cancellationToken = default);
    }
}
