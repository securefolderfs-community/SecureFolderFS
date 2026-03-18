using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using SecureFolderFS.Shared.ComponentModel;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Chunks.AesCtrHmac;
using static SecureFolderFS.Core.Cryptography.Extensions.ContentCryptExtensions.AesCtrHmacContentExtensions;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.AesCtrHmacHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.ContentCrypt
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal sealed class AesCtrHmacContentCrypt : BaseContentCrypt
    {
        private readonly IKeyUsage _macKey;

        /// <inheritdoc/>
        public override int ChunkPlaintextSize { get; } = CHUNK_PLAINTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkCiphertextSize { get; } = CHUNK_CIPHERTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkFirstReservedSize { get; } = CHUNK_NONCE_SIZE;

        public AesCtrHmacContentCrypt(IKeyUsage macKey)
        {
            _macKey = macKey;
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override unsafe void EncryptChunk(ReadOnlySpan<byte> plaintextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk)
        {
            // Chunk nonce
            RandomNumberGenerator.Fill(ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE));

            // Encrypt
            AesCtr128.Encrypt(
                plaintextChunk,
                header.GetHeaderContentKey(),
                ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE),
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE, plaintextChunk.Length));

            // Calculate MAC using UseKey pattern
            fixed (byte* headerPtr = header)
            fixed (byte* ciphertextPtr = ciphertextChunk)
            {
                var state = (
                    headerPtr: (nint)headerPtr,
                    headerLen: header.Length,
                    ctPtr: (nint)ciphertextPtr,
                    ctLen: ciphertextChunk.Length,
                    ptLen: plaintextChunk.Length,
                    chunkNumber
                );

                _macKey.UseKey(state, static (macKey, s) =>
                {
                    var hdr = new ReadOnlySpan<byte>((byte*)s.headerPtr, s.headerLen);
                    var ct = new Span<byte>((byte*)s.ctPtr, s.ctLen);

                    CalculateChunkMacStatic(
                        macKey,
                        hdr.GetHeaderNonce(),
                        ct.Slice(0, CHUNK_NONCE_SIZE),
                        ct.Slice(CHUNK_NONCE_SIZE, s.ptLen),
                        s.chunkNumber,
                        ct.Slice(CHUNK_NONCE_SIZE + s.ptLen, CHUNK_MAC_SIZE));
                });
            }
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override unsafe bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> plaintextChunk)
        {
            // Verify MAC using UseKey pattern
            fixed (byte* headerPtr = header)
            fixed (byte* ciphertextPtr = ciphertextChunk)
            {
                var state = (
                    headerPtr: (nint)headerPtr,
                    headerLen: header.Length,
                    ctPtr: (nint)ciphertextPtr,
                    ctLen: ciphertextChunk.Length,
                    chunkNumber
                );

                var macValid = _macKey.UseKey(state, static (macKey, s) =>
                {
                    var hdr = new ReadOnlySpan<byte>((byte*)s.headerPtr, s.headerLen);
                    var ct = new ReadOnlySpan<byte>((byte*)s.ctPtr, s.ctLen);

                    // Allocate byte* for MAC
                    Span<byte> mac = stackalloc byte[CHUNK_MAC_SIZE];

                    // Calculate MAC
                    CalculateChunkMacStatic(
                        macKey,
                        hdr.GetHeaderNonce(),
                        ct.GetChunkNonce(),
                        ct.GetChunkPayload(),
                        s.chunkNumber,
                        mac);

                    // Check MAC using constant-time comparison to prevent timing attacks
                    return CryptographicOperations.FixedTimeEquals(mac, ct.GetChunkMac());
                });

                if (!macValid)
                    return false;
            }

            // Decrypt
            AesCtr128.Decrypt(
                ciphertextChunk.GetChunkPayload(),
                header.GetHeaderContentKey(),
                ciphertextChunk.GetChunkNonce(),
                plaintextChunk);

            return true;
        }

        [SkipLocalsInit]
        private static void CalculateChunkMacStatic(ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> headerNonce, ReadOnlySpan<byte> chunkNonce, ReadOnlySpan<byte> ciphertextPayload, long chunkNumber, Span<byte> chunkMac)
        {
            // Convert long to byte array
            Span<byte> beChunkNumber = stackalloc byte[sizeof(long)];
            Unsafe.As<byte, long>(ref beChunkNumber[0]) = chunkNumber;

            // Reverse byte order as needed
            if (BitConverter.IsLittleEndian)
                beChunkNumber.Reverse();

            // Initialize HMAC
            using var hmacSha256 = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey);

            hmacSha256.AppendData(headerNonce);         // headerNonce
            hmacSha256.AppendData(beChunkNumber);       // beChunkNumber
            hmacSha256.AppendData(chunkNonce);          // chunkNonce
            hmacSha256.AppendData(ciphertextPayload);   // ciphertextPayload

            hmacSha256.GetCurrentHash(chunkMac);
        }
    }
}
