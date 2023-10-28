using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Headers.AesCtrHmac;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.AesCtrHmacHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal sealed class AesCtrHmacHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderCleartextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public AesCtrHmacHeaderCrypt(SecretKey encKey, SecretKey macKey)
            : base(encKey, macKey)
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
            AesCtr128.Encrypt(
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
            AesCtr128.Decrypt(
                ciphertextHeader.GetHeaderContentKey(),
                encKey,
                ciphertextHeader.GetHeaderNonce(),
                cleartextHeader.Slice(HEADER_NONCE_SIZE));

            return true;
        }

        private void CalculateHeaderMac(ReadOnlySpan<byte> headerNonce, ReadOnlySpan<byte> ciphertextPayload, Span<byte> headerMac)
        {
            // Initialize HMAC
            using var hmacSha256 = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey);

            hmacSha256.AppendData(headerNonce);         // headerNonce
            hmacSha256.AppendData(ciphertextPayload);   // ciphertextPayload

            hmacSha256.GetCurrentHash(headerMac);
        }
    }
}
