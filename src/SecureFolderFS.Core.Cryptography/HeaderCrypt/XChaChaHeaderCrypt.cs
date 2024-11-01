using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Headers.XChaCha20Poly1305;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.XChaChaHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal sealed class XChaChaHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderPlaintextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public XChaChaHeaderCrypt(SecretKey encKey, SecretKey macKey)
            : base(encKey, macKey)
        {
        }

        /// <inheritdoc/>
        public override void CreateHeader(Span<byte> plaintextHeader)
        {
            // Nonce
            secureRandom.GetNonZeroBytes(plaintextHeader.Slice(0, HEADER_NONCE_SIZE));

            // Content key
            secureRandom.GetBytes(plaintextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE));
        }

        /// <inheritdoc/>
        public override void EncryptHeader(ReadOnlySpan<byte> plaintextHeader, Span<byte> ciphertextHeader)
        {
            // Nonce
            plaintextHeader.GetHeaderNonce().CopyTo(ciphertextHeader);

            // Encrypt
            XChaCha20Poly1305.Encrypt(
                plaintextHeader.GetHeaderContentKey(),
                encKey,
                plaintextHeader.GetHeaderNonce(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE),
                default);
        }

        /// <inheritdoc/>
        public override bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader)
        {
            // Nonce
            ciphertextHeader.GetHeaderNonce().CopyTo(plaintextHeader);

            // Decrypt
            return XChaCha20Poly1305.Decrypt(
                ciphertextHeader.Slice(HEADER_NONCE_SIZE),
                encKey,
                ciphertextHeader.GetHeaderNonce(),
                plaintextHeader.Slice(HEADER_NONCE_SIZE),
                default);
        }
    }
}
