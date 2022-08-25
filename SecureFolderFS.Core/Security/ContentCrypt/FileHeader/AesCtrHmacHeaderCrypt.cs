using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Constants.Security.Headers.AesCtrHmac;
using static SecureFolderFS.Core.Extensions.SecurityExtensions.Header.AesCtrHmacHeaderExtensions;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    /// <inheritdoc cref="IHeaderCrypt"/>
    internal sealed class AesCtrHmacHeaderCrypt : BaseHeaderCrypt
    {
        /// <inheritdoc/>
        public override int HeaderCiphertextSize { get; } = HEADER_SIZE;

        /// <inheritdoc/>
        public override int HeaderCleartextSize { get; } = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE;

        public AesCtrHmacHeaderCrypt(MasterKey masterKey, ICipherProvider cipherProvider)
            : base(masterKey, cipherProvider)
        {
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override void CreateHeader(Span<byte> cleartextHeader)
        {
            // Nonce
            secureRandom.GetNonZeroBytes(cleartextHeader.Slice(0, HEADER_NONCE_SIZE));

            // Content key
            secureRandom.GetBytes(cleartextHeader.Slice(HEADER_NONCE_SIZE));
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override void EncryptHeader(ReadOnlySpan<byte> cleartextHeader, Span<byte> ciphertextHeader)
        {
            var encKey = masterKey.GetEncryptionKey();

            // Nonce
            cleartextHeader.Slice(0, HEADER_NONCE_SIZE).CopyTo(ciphertextHeader);

            // Encrypt
            cipherProvider.AesCtrCrypt.Encrypt(
                cleartextHeader.GetHeaderContentKey(),
                encKey,
                cleartextHeader.GetHeaderNonce(),
                ciphertextHeader.Slice(HEADER_NONCE_SIZE));

            // Calculate MAC
            CalculateHeaderMac(
                cleartextHeader.GetHeaderNonce(),
                cleartextHeader.GetHeaderContentKey(),
                ciphertextHeader.Slice(cleartextHeader.Length)); // cleartextHeader.Length already includes HEADER_NONCE_SIZE
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> cleartextHeader)
        {
            var encKey = masterKey.GetEncryptionKey();

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

        [SkipLocalsInit]
        private void CalculateHeaderMac(ReadOnlySpan<byte> headerNonce, ReadOnlySpan<byte> ciphertextPayload, Span<byte> headerMac)
        {
            var macKey = masterKey.GetMacKey();
            using var hmacSha256 = cipherProvider.GetHmacInstance();

            hmacSha256.InitializeHmac(macKey);
            hmacSha256.Update(headerNonce);
            hmacSha256.DoFinal(ciphertextPayload);

            hmacSha256.GetHash(headerMac);
        }
    }
}
