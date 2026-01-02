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
        public override int HeaderPlaintextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public AesCtrHmacHeaderCrypt(KeyPair keyPair)
            : base(keyPair)
        {
        }

        /// <inheritdoc/>
        public override void CreateHeader(Span<byte> plaintextHeader)
        {
            // Nonce
            RandomNumberGenerator.Fill(plaintextHeader.Slice(0, HEADER_NONCE_SIZE));

            // Content key
            RandomNumberGenerator.Fill(plaintextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE));
        }

        /// <inheritdoc/>
        public override void EncryptHeader(ReadOnlySpan<byte> plaintextHeader, Span<byte> ciphertextHeader)
        {
            // Nonce
            plaintextHeader.Slice(0, HEADER_NONCE_SIZE).CopyTo(ciphertextHeader);

            // Encrypt
            AesCtr128.Encrypt(
                plaintextHeader.GetHeaderContentKey(),
                DekKey,
                plaintextHeader.GetHeaderNonce(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE));

            // Calculate MAC
            CalculateHeaderMac(
                plaintextHeader.GetHeaderNonce(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE),
                ciphertextHeader.Slice(plaintextHeader.Length)); // plaintextHeader.Length already includes HEADER_NONCE_SIZE
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader)
        {
            // Allocate byte* for MAC
            Span<byte> mac = stackalloc byte[HEADER_MAC_SIZE];

            // Calculate MAC
            CalculateHeaderMac(
                ciphertextHeader.GetHeaderNonce(),
                ciphertextHeader.GetHeaderContentKey(),
                mac);

            // Check MAC using constant-time comparison to prevent timing attacks
            if (!CryptographicOperations.FixedTimeEquals(mac, ciphertextHeader.GetHeaderMac()))
                return false;

            // Nonce
            ciphertextHeader.GetHeaderNonce().CopyTo(plaintextHeader);

            // Decrypt
            AesCtr128.Decrypt(
                ciphertextHeader.GetHeaderContentKey(),
                DekKey,
                ciphertextHeader.GetHeaderNonce(),
                plaintextHeader.Slice(HEADER_NONCE_SIZE));

            return true;
        }

        private void CalculateHeaderMac(ReadOnlySpan<byte> headerNonce, ReadOnlySpan<byte> ciphertextPayload, Span<byte> headerMac)
        {
            // Initialize HMAC
            using var hmacSha256 = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, MacKey);

            hmacSha256.AppendData(headerNonce);         // headerNonce
            hmacSha256.AppendData(ciphertextPayload);   // ciphertextPayload

            hmacSha256.GetCurrentHash(headerMac);
        }
    }
}
