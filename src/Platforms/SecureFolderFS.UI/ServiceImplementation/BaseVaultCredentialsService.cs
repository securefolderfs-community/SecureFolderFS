using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
            // Default to AES-GCM if supported, otherwise use XChaCha20-Poly1305.
            // This is because AES-NI intrinsics are faster on supported platforms
            if (AesGcm.IsSupported)
                yield return Core.Cryptography.Constants.CipherId.AES_GCM;

#if DEBUG
            if (!OperatingSystem.IsIOS())
                yield return Core.Cryptography.Constants.CipherId.XCHACHA20_POLY1305;
            
            if (!AesGcm.IsSupported)
                yield return Core.Cryptography.Constants.CipherId.AES_GCM; // Brute-force the branch anyway
#else
            yield return Core.Cryptography.Constants.CipherId.XCHACHA20_POLY1305;
#endif

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
            AuthenticationViewModel[]? requiredProcedures = null;
            AuthenticationViewModel[]? complementedProcedures = null;

            try
            {
                requiredProcedures = await GetLoginAsync(vaultFolder, unlockProcedure with { Complementation = null }, string.Empty, cancellationToken).ToArrayAsyncImpl(cancellationToken);
                var requiredText = string.Join(" + ", requiredProcedures.Select(x => x.Title));

                if (string.IsNullOrWhiteSpace(unlockProcedure.Complementation))
                    return requiredText;

                complementedProcedures = await GetLoginAsync(vaultFolder, new([ unlockProcedure.Complementation ], null), string.Empty, cancellationToken).ToArrayAsyncImpl(cancellationToken);
                var complementedText = string.Join(" + ", complementedProcedures.Select(x => x.Title));

                return string.IsNullOrWhiteSpace(requiredText)
                    ? complementedText
                    : $"{requiredText} / {complementedText}";
            }
            catch (Exception)
            {
                return "AuthenticationUnavailable".ToLocalized();
            }
            finally
            {
                requiredProcedures?.DisposeAll();
                complementedProcedures?.DisposeAll();
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

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(
            IFolder vaultFolder,
            AuthenticationMethod unlockProcedure,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await foreach (var item in GetLoginAsync(vaultFolder, unlockProcedure, config.Uid, cancellationToken))
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

        protected static IEnumerable<string> EnumerateLoginMethods(AuthenticationMethod unlockProcedure)
        {
            foreach (var item in unlockProcedure.Methods)
                yield return item;

            if (!string.IsNullOrWhiteSpace(unlockProcedure.Complementation))
                yield return unlockProcedure.Complementation;
        }
    }
}
