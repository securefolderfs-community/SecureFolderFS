using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Headers.AesGcm;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.AesGcmHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal sealed class AesGcmHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderPlaintextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public AesGcmHeaderCrypt(KeyPair keyPair)
            : base(keyPair)
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
            AesGcm128.Encrypt(
                plaintextHeader.GetHeaderContentKey(),
                DekKey,
                plaintextHeader.GetHeaderNonce(),
                ciphertextHeader.GetHeaderTag(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE),
                default);
        }

        /// <inheritdoc/>
        public override bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader)
        {
            // Nonce
            ciphertextHeader.GetHeaderNonce().CopyTo(plaintextHeader);

            // Decrypt
            return AesGcm128.TryDecrypt(
                ciphertextHeader.GetHeaderContentKey(),
                DekKey,
                ciphertextHeader.GetHeaderNonce(),
                ciphertextHeader.GetHeaderTag(),
                plaintextHeader.Slice(HEADER_NONCE_SIZE),
                default);
        }
    }
}
