using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal abstract class BaseHeaderCrypt : IHeaderCrypt
    {
        protected readonly SecretKey encKey;
        protected readonly SecretKey macKey;
        protected readonly RandomNumberGenerator secureRandom;

        /// <inheritdoc/>
        public abstract int HeaderCiphertextSize { get; }

        /// <inheritdoc/>
        public abstract int HeaderCleartextSize { get; }

        protected BaseHeaderCrypt(SecretKey encKey, SecretKey macKey)
        {
            this.encKey = encKey;
            this.macKey = macKey;
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
