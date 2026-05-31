using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultManagerService
    {
        /// <summary>
        /// Creates new or overwrites an existing vault in the specified <paramref name="vaultFolder"/>.
        /// </summary>
        /// <remarks>
        /// To retrieve the decryption key, call the <c>.ToString()</c> method
        /// on the returned <see cref="IDisposable"/> instance.
        /// Since the key returned by this method can be used to decrypt vault contents
        /// regardless of whether the vault passkey was changed, it is, by nature,
        /// very sensitive and should be disposed of as soon as it is no longer needed.
        /// </remarks>
        /// <param name="vaultFolder">The folder where the vault should be created.</param>
        /// <param name="passkey">The passkey represented by <see cref="IEnumerable{T}"/> of <see cref="IDisposable"/> representing authentication elements to set for this vault.</param>
        /// <param name="vaultOptions">The required options to set for this vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> CreateAsync(IFolder vaultFolder, IKeyUsage passkey, VaultOptions vaultOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks the specified <paramref name="vaultFolder"/> using the provided <paramref name="passkey"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="passkey"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> UnlockAsync(IFolder vaultFolder, IKeyUsage passkey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recovers the specified <paramref name="vaultFolder"/> using the provided <paramref name="encodedRecoveryKey"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="encodedRecoveryKey">The Base64 encoded recovery key.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> RecoverAsync(IFolder vaultFolder, string encodedRecoveryKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores the specified <paramref name="vaultFolder"/> using the provided <paramref name="encodedRecoveryKey"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="encodedRecoveryKey">The Base64 encoded recovery key.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the recovery key used to decrypt the vault.</returns>
        Task<IDisposable> RestoreAsync(IFolder vaultFolder, string encodedRecoveryKey, CancellationToken cancellationToken = default);

        // TODO: Consider using IVaultUnlockingModel
        //Task<IVaultUnlockingModel> GetUnlockingModelAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new App Platform vault. Generates DEK+MAC internally (no password, no keystore.cfg).
        /// Returns the security wrapper and raw key bytes for encryption and upload to the server.
        /// </summary>
        /// <param name="vaultFolder">The folder where the vault should be created.</param>
        /// <param name="vaultOptions">The required options to set for this vault (must have AppPlatform set).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is a tuple of (unlockContract, dekKey, macKey).</returns>
        Task<(IDisposable UnlockContract, IKeyUsage DekKey, IKeyUsage MacKey)> CreateAppPlatformAsync(IFolder vaultFolder, VaultOptions vaultOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlocks an App Platform vault using the combined DEK+MAC key from the server.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="passkey">The combined DEK‖MAC key (64 bytes) obtained from the server-brokered key chain.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IDisposable"/> that represents the unlock contract.</returns>
        Task<IDisposable> UnlockAppPlatformAsync(IFolder vaultFolder, IKeyUsage passkey, CancellationToken cancellationToken = default);

        Task ModifyComplementationAsync(IFolder vaultFolder, IDisposable unlockContract, ComplementationCredentials credentials, VaultOptions vaultOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Modifies the configured authentication for the specified <paramref name="vaultFolder"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="unlockContract">The recovery key used to decrypt the vault</param>
        /// <param name="newPasskey">The new passkey to secure the vault with.</param>
        /// <param name="vaultOptions">>The required options to set for this vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ModifyAuthenticationAsync(IFolder vaultFolder, IDisposable unlockContract, IKeyUsage newPasskey, VaultOptions vaultOptions, CancellationToken cancellationToken = default);

        /// <summary>
        /// Modifies the configured authentication for the specified <paramref name="vaultFolder"/>
        /// using both old and new passkeys.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <param name="unlockContract">The recovery key used to decrypt the vault</param>
        /// <param name="oldPasskey">The currently configured passkey.</param>
        /// <param name="newPasskey">The new passkey to secure the vault with.</param>
        /// <param name="vaultOptions">The required options to set for this vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ModifyAuthenticationAsync(IFolder vaultFolder, IDisposable unlockContract, IKeyUsage oldPasskey, IKeyUsage newPasskey, VaultOptions vaultOptions, CancellationToken cancellationToken = default);
    }
}
