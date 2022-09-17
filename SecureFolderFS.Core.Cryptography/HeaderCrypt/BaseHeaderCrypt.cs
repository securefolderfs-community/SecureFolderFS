using SecureFolderFS.Core.Cryptography.Cipher;
using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal abstract class BaseHeaderCrypt : IHeaderCrypt
    {
        protected readonly SecretKey macKey;
        protected readonly SecretKey encryptionKey;
        protected readonly ICipherProvider cipherProvider;
        protected readonly RandomNumberGenerator secureRandom;

        /// <inheritdoc/>
        public abstract int HeaderCiphertextSize { get; }

        /// <inheritdoc/>
        public abstract int HeaderCleartextSize { get; }

        protected BaseHeaderCrypt(SecretKey macKey, SecretKey encryptionKey, ICipherProvider cipherProvider)
        {
            this.macKey = macKey;
            this.encryptionKey = encryptionKey;
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
