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
        public override unsafe void EncryptHeader(ReadOnlySpan<byte> plaintextHeader, Span<byte> ciphertextHeader)
        {
            // Nonce
            plaintextHeader.Slice(0, HEADER_NONCE_SIZE).CopyTo(ciphertextHeader);

            // Use unsafe pointers to pass span data through the UseKey callback
            fixed (byte* plaintextPtr = plaintextHeader)
            fixed (byte* ciphertextPtr = ciphertextHeader)
            {
                var state = (ptPtr: (nint)plaintextPtr, ptLen: plaintextHeader.Length, ctPtr: (nint)ciphertextPtr, ctLen: ciphertextHeader.Length);

                // Encrypt with DekKey
                DekKey.UseKey(state, static (dekKey, s) =>
                {
                    var pt = new ReadOnlySpan<byte>((byte*)s.ptPtr, s.ptLen);
                    var ct = new Span<byte>((byte*)s.ctPtr, s.ctLen);

                    AesCtr128.Encrypt(
                        pt.GetHeaderContentKey(),
                        dekKey,
                        pt.GetHeaderNonce(),
                        ct.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE));
                });

                // Calculate MAC with MacKey
                MacKey.UseKey(state, static (macKey, s) =>
                {
                    var pt = new ReadOnlySpan<byte>((byte*)s.ptPtr, s.ptLen);
                    var ct = new Span<byte>((byte*)s.ctPtr, s.ctLen);

                    CalculateHeaderMacInternal(
                        macKey,
                        pt.GetHeaderNonce(),
                        ct.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE),
                        ct.Slice(pt.Length)); // plaintextHeader.Length already includes HEADER_NONCE_SIZE
                });
            }
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override unsafe bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader)
        {
            // Use unsafe pointers to pass span data through the UseKey callback
            fixed (byte* ciphertextPtr = ciphertextHeader)
            fixed (byte* plaintextPtr = plaintextHeader)
            {
                var state = (ctPtr: (nint)ciphertextPtr, ctLen: ciphertextHeader.Length, ptPtr: (nint)plaintextPtr, ptLen: plaintextHeader.Length);

                // Verify MAC with MacKey
                var macValid = MacKey.UseKey(state, static (macKey, s) =>
                {
                    var ct = new ReadOnlySpan<byte>((byte*)s.ctPtr, s.ctLen);

                    // Allocate byte* for MAC
                    Span<byte> mac = stackalloc byte[HEADER_MAC_SIZE];

                    // Calculate MAC
                    CalculateHeaderMacInternal(
                        macKey,
                        ct.GetHeaderNonce(),
                        ct.GetHeaderContentKey(),
                        mac);

                    // Check MAC using constant-time comparison to prevent timing attacks
                    return CryptographicOperations.FixedTimeEquals(mac, ct.GetHeaderMac());
                });

                if (!macValid)
                    return false;

                // Nonce
                ciphertextHeader.GetHeaderNonce().CopyTo(plaintextHeader);

                // Decrypt with DekKey
                DekKey.UseKey(state, static (dekKey, s) =>
                {
                    var ct = new ReadOnlySpan<byte>((byte*)s.ctPtr, s.ctLen);
                    var pt = new Span<byte>((byte*)s.ptPtr, s.ptLen);

                    AesCtr128.Decrypt(
                        ct.GetHeaderContentKey(),
                        dekKey,
                        ct.GetHeaderNonce(),
                        pt.Slice(HEADER_NONCE_SIZE));
                });

                return true;
            }
        }

        private static void CalculateHeaderMacInternal(ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> headerNonce, ReadOnlySpan<byte> ciphertextPayload, Span<byte> headerMac)
        {
            // Initialize HMAC
            using var hmacSha256 = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey);

            hmacSha256.AppendData(headerNonce);         // headerNonce
            hmacSha256.AppendData(ciphertextPayload);   // ciphertextPayload

            hmacSha256.GetCurrentHash(headerMac);
        }
    }
}
