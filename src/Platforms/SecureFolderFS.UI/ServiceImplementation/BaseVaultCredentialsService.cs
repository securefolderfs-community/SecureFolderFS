using System;
using System.Collections.Generic;
using System.Threading;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCredentialsService"/>
    public abstract class BaseVaultCredentialsService : IVaultCredentialsService
    {
        /// <inheritdoc/>
        public virtual IEnumerable<string> GetEncodingOptions()
        {
            if (!OperatingSystem.IsIOS())
                yield return Core.Cryptography.Constants.CipherId.ENCODING_BASE4K;

            yield return Core.Cryptography.Constants.CipherId.ENCODING_BASE64URL;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetContentCiphers()
        {
            if (!OperatingSystem.IsIOS())
                yield return Core.Cryptography.Constants.CipherId.XCHACHA20_POLY1305;

            yield return Core.Cryptography.Constants.CipherId.AES_GCM;

#if DEBUG
            yield return Core.Cryptography.Constants.CipherId.NONE;
#endif
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> GetFileNameCiphers()
        {
            yield return Core.Cryptography.Constants.CipherId.AES_SIV;
            yield return Core.Cryptography.Constants.CipherId.NONE;
        }

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, CancellationToken cancellationToken = default);

        public abstract IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, CancellationToken cancellationToken = default);
    }
}
