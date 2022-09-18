using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.Cryptography.NameCrypt;

namespace SecureFolderFS.Core.Security
{
    /// <inheritdoc cref="ISecurity"/>
    internal sealed class Security : ISecurity
    {
        /// <inheritdoc/>
        public ICipherProvider CipherProvider { get; }

        /// <inheritdoc/>
        public IContentCrypt ContentCrypt { get; }

        /// <inheritdoc/>
        public IHeaderCrypt HeaderCrypt { get; }

        /// <inheritdoc/>
        public INameCrypt? NameCrypt { get; }

        public Security(ICipherProvider cipherProvider, IContentCrypt contentCrypt, IHeaderCrypt headerCrypt, INameCrypt? nameCrypt)
        {
            CipherProvider = cipherProvider;
            ContentCrypt = contentCrypt;
            HeaderCrypt = headerCrypt;
            NameCrypt = nameCrypt;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ContentCrypt.Dispose();
            HeaderCrypt.Dispose();
        }
    }
}
