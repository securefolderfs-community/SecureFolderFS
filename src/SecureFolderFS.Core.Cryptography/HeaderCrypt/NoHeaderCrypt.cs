using System;
using SecureFolderFS.Core.Cryptography.SecureStore;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Headers.Empty;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal sealed class NoHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderPlaintextSize { get; } = HEADER_SIZE;

        public NoHeaderCrypt(KeyPair keyPair)
            : base(keyPair)
        {
        }

        /// <inheritdoc/>
        public override void CreateHeader(Span<byte> plaintextHeader)
        {
            _ = plaintextHeader;
        }

        /// <inheritdoc/>
        public override void EncryptHeader(ReadOnlySpan<byte> plaintextHeader, Span<byte> ciphertextHeader)
        {
            plaintextHeader.CopyTo(ciphertextHeader);
        }

        /// <inheritdoc/>
        public override bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader)
        {
            ciphertextHeader.CopyTo(plaintextHeader);
            return true;
        }
    }
}
