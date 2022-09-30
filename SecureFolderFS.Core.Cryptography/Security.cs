using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.Cryptography.HeaderCrypt;
using SecureFolderFS.Core.Cryptography.NameCrypt;
using System;

namespace SecureFolderFS.Core.Cryptography
{
    /// <summary>
    /// Represents a security object used for encrypting and decrypting data in SecureFolderFS.
    /// </summary>
    public sealed class Security : IDisposable
    {
        // TODO: Needs docs

        public CipherProvider CipherProvider { get; }

        public IContentCrypt ContentCrypt { get; }

        public IHeaderCrypt HeaderCrypt { get; }

        public INameCrypt? NameCrypt { get; }

        private Security(CipherProvider cipherProvider, IContentCrypt contentCrypt, IHeaderCrypt headerCrypt, INameCrypt? nameCrypt)
        {
            CipherProvider = cipherProvider;
            ContentCrypt = contentCrypt;
            HeaderCrypt = headerCrypt;
            NameCrypt = nameCrypt;
        }

        public static Security CreateNew()
        {

        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ContentCrypt.Dispose();
            HeaderCrypt.Dispose();
        }
    }
}
