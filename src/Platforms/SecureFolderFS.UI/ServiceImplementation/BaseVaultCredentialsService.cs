using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.MemoryStorageEx;

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
        public virtual async Task<string> FromUnlockProcedureAsync(AuthenticationMethod unlockProcedure, CancellationToken cancellationToken = default)
        {
            try
            {
                var emptyFolder = new MemoryFolderEx(string.Empty, string.Empty, null);
                var procedures = await GetCreationAsync(emptyFolder, string.Empty, cancellationToken).ToArrayAsyncImpl(cancellationToken);
                var result = unlockProcedure.Methods
                    .Select(method => procedures.FirstOrDefault(p => p.Id == method)?.Title)
                    .Where(title => title is not null);

                procedures.DisposeAll();
                return string.Join(" + ", result);
            }
            catch (Exception)
            {
                return "AuthenticationUnavailable".ToLocalized();
            }
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        public abstract IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
