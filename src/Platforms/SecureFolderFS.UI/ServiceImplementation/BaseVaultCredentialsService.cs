using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCredentialsService"/>
    public abstract class BaseVaultCredentialsService : IVaultCredentialsService
    {
        /// <inheritdoc/>
        public virtual IEnumerable<string> GetEncodingOptions()
        {
            yield return Core.Cryptography.Constants.CipherId.ENCODING_BASE4K;
            yield return Core.Cryptography.Constants.CipherId.ENCODING_BASE64URL;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetContentCiphers()
        {
            if (!OperatingSystem.IsIOS())
                yield return Core.Cryptography.Constants.CipherId.XCHACHA20_POLY1305;

            yield return Core.Cryptography.Constants.CipherId.AES_GCM;
            yield return Core.Cryptography.Constants.CipherId.NONE;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetFileNameCiphers()
        {
            yield return Core.Cryptography.Constants.CipherId.AES_SIV;
            yield return Core.Cryptography.Constants.CipherId.NONE;
        }

        /// <inheritdoc/>
        public virtual async Task<string> FromUnlockProcedureAsync(IFolder vaultFolder, AuthenticationMethod unlockProcedure, CancellationToken cancellationToken = default)
        {
            try
            {
                var procedures = await GetLoginAsync(vaultFolder, unlockProcedure, string.Empty, cancellationToken).ToArrayAsyncImpl(cancellationToken);
                var result = string.Join(" + ", procedures.Select(x => x.Title));

                procedures.DisposeAll();
                return result;
            }
            catch (Exception)
            {
                return "AuthenticationUnavailable".ToLocalized();
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var authenticationMethod = AuthenticationMethod.FromString(config.AuthenticationMethod);

            cancellationToken.ThrowIfCancellationRequested();
            await foreach (var item in GetLoginAsync(vaultFolder, authenticationMethod, config.Uid, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }
        }

        /// <summary>
        /// Retrieves the authentication methods for login routine.
        /// </summary>
        /// <param name="vaultFolder">The folder representing the vault for which authentication methods should be retrieved.</param>
        /// <param name="unlockProcedure">The authentication method used to unlock the vault.</param>
        /// <param name="vaultId">The unique identifier of the vault.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="AuthenticationViewModel"/> representing available authentication options.</returns>
        protected abstract IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, AuthenticationMethod unlockProcedure, string vaultId, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
