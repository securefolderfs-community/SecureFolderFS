using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services.Vault
{
    /// <summary>
    /// Represents an interface used for creating vaults.
    /// </summary>
    public interface IVaultCreator
    {
        /// <summary>
        /// Creates new or overwrites an existing vault in the specified <paramref name="vaultFolder"/>.
        /// </summary>
        /// <remarks>
        /// To retrieve the decryption key, call the <c>.ToString()</c> method
        /// on the returned <see cref="IDisposable"/> instance.
        /// Since the key returned by this method can be used to decrypt vault contents
        /// regardless of whether or not the vault password was changed, it is, by nature,
        /// very sensitive and should be disposed of as soon as it is no longer needed.
        /// </remarks>
        /// <param name="vaultFolder">The folder where the vault should be created.</param>
        /// <param name="password">The user password to set for this vault.</param>
        /// <param name="nameCipherId">The filename cipher scheme to set.</param>
        /// <param name="contentCipherId">The content cipher scheme to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the master key used to decrypt the vault.</returns>
        Task<IDisposable> CreateVaultAsync(IFolder vaultFolder, IPassword password, string nameCipherId, string contentCipherId, CancellationToken cancellationToken = default);
    }
}
