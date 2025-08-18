using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal abstract class BaseHeaderCrypt : IHeaderCrypt
    {
        protected readonly KeyPair keyPair;
        protected readonly RandomNumberGenerator secureRandom;

        protected SecretKey DekKey => keyPair.DekKey;

        protected SecretKey MacKey => keyPair.MacKey;

        /// <inheritdoc/>
        public abstract int HeaderCiphertextSize { get; }

        /// <inheritdoc/>
        public abstract int HeaderPlaintextSize { get; }

        protected BaseHeaderCrypt(KeyPair keyPair)
        {
            this.keyPair = keyPair;
            this.secureRandom = RandomNumberGenerator.Create();
        }

        /// <inheritdoc/>
        public abstract void CreateHeader(Span<byte> plaintextHeader);

        /// <inheritdoc/>
        public abstract void EncryptHeader(ReadOnlySpan<byte> plaintextHeader, Span<byte> ciphertextHeader);

        /// <inheritdoc/>
        public abstract bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            secureRandom.Dispose();
        }
    }
}
