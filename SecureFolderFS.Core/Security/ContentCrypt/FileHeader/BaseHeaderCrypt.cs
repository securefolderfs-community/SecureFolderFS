using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal abstract class BaseHeaderCrypt : IHeaderCrypt
    {
        protected readonly MasterKey masterKey;
        protected readonly ICipherProvider cipherProvider;
        protected readonly RandomNumberGenerator secureRandom;

        /// <inheritdoc/>
        public abstract int HeaderCiphertextSize { get; }

        protected BaseHeaderCrypt(MasterKey masterKey, ICipherProvider cipherProvider)
        {
            this.masterKey = masterKey;
            this.cipherProvider = cipherProvider;
            this.secureRandom = RandomNumberGenerator.Create();
        }

        /// <inheritdoc/>
        public abstract void CreateHeader(Span<byte> cleartextHeader);

        /// <inheritdoc/>
        public abstract void EncryptHeader(ReadOnlySpan<byte> cleartextHeader, Span<byte> ciphertextHeader);

        /// <inheritdoc/>
        public abstract bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> cleartextHeader);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            secureRandom.Dispose();
        }
    }
}
