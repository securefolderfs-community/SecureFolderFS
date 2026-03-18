using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Security.Cryptography;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal abstract class BaseHeaderCrypt : IHeaderCrypt
    {
        private readonly KeyPair _keyPair;

        protected IKeyUsage DekKey => _keyPair.DekKey;

        protected IKeyUsage MacKey => _keyPair.MacKey;

        /// <inheritdoc/>
        public abstract int HeaderCiphertextSize { get; }

        /// <inheritdoc/>
        public abstract int HeaderPlaintextSize { get; }

        protected BaseHeaderCrypt(KeyPair keyPair)
        {
            _keyPair = keyPair;
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
        }
    }
}
