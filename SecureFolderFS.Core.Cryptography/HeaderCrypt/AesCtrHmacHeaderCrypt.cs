using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Cryptography.Constants.Crypt.Headers.AesCtrHmac;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.AesCtrHmacHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    public sealed class AesCtrHmacHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderCleartextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public AesCtrHmacHeaderCrypt(SecretKey encKey, SecretKey macKey, CipherProvider cipherProvider)
            : base(encKey, macKey, cipherProvider)
        {
        }

        /// <inheritdoc/>
        public override void CreateHeader(Span<byte> cleartextHeader)
        {
            // Nonce
            secureRandom.GetNonZeroBytes(cleartextHeader.Slice(0, HEADER_NONCE_SIZE));

            // Content key
            secureRandom.GetBytes(cleartextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE));
        }

        /// <inheritdoc/>
        public override void EncryptHeader(ReadOnlySpan<byte> cleartextHeader, Span<byte> ciphertextHeader)
        {
            // Nonce
            cleartextHeader.Slice(0, HEADER_NONCE_SIZE).CopyTo(ciphertextHeader);

            // Encrypt
            cipherProvider.AesCtrCrypt.Encrypt(
                cleartextHeader.GetHeaderContentKey(),
                encKey,
                cleartextHeader.GetHeaderNonce(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE));

            // Calculate MAC
            CalculateHeaderMac(
                cleartextHeader.GetHeaderNonce(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE),
                ciphertextHeader.Slice(cleartextHeader.Length)); // cleartextHeader.Length already includes HEADER_NONCE_SIZE
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> cleartextHeader)
        {
            // Allocate byte* for MAC
            Span<byte> mac = stackalloc byte[HEADER_MAC_SIZE];

            // Calculate MAC
            CalculateHeaderMac(
                ciphertextHeader.GetHeaderNonce(),
                ciphertextHeader.GetHeaderContentKey(),
                mac);

            // Check MAC
            if (!mac.SequenceEqual(ciphertextHeader.GetHeaderMac()))
                return false;

            // Nonce
            ciphertextHeader.GetHeaderNonce().CopyTo(cleartextHeader);

            // Decrypt
            cipherProvider.AesCtrCrypt.Decrypt(
                ciphertextHeader.GetHeaderContentKey(),
                encKey,
                ciphertextHeader.GetHeaderNonce(),
                cleartextHeader.Slice(HEADER_NONCE_SIZE));

            return true;
        }

        private void CalculateHeaderMac(ReadOnlySpan<byte> headerNonce, ReadOnlySpan<byte> ciphertextPayload, Span<byte> headerMac)
        {
            using var hmacSha256 = cipherProvider.HmacSha256Crypt.GetInstance();

            hmacSha256.InitializeHmac(macKey);
            hmacSha256.Update(headerNonce);
            hmacSha256.Update(ciphertextPayload);
            hmacSha256.GetHash(headerMac);
        }
    }
}
