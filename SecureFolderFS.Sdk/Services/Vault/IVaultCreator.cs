using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Storage;
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
        /// regardless of whether or not the vault passkey was changed, it is, by nature,
        /// very sensitive and should be disposed of as soon as it is no longer needed.
        /// </remarks>
        /// <param name="vaultFolder">The folder where the vault should be created.</param>
        /// <param name="passkey">The compiled passkey to set for this vault.</param>
        /// <param name="vaultOptions">The required options to set for this vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the master key used to decrypt the vault.</returns>
        Task<IDisposable> CreateVaultAsync(IFolder vaultFolder, IDisposable passkey, VaultOptions vaultOptions, CancellationToken cancellationToken = default);
    }
}
