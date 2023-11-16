﻿using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Chunks.AesCtrHmac;
using static SecureFolderFS.Core.Cryptography.Extensions.ContentCryptExtensions.AesCtrHmacContentExtensions;
using static SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions.AesCtrHmacHeaderExtensions;

namespace SecureFolderFS.Core.Cryptography.ContentCrypt
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal sealed class AesCtrHmacContentCrypt : BaseContentCrypt
    {
        private readonly SecretKey _macKey;

        /// <inheritdoc/>
        public override int ChunkCleartextSize { get; } = CHUNK_CLEARTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkCiphertextSize { get; } = CHUNK_CIPHERTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkFirstReservedSize { get; } = CHUNK_NONCE_SIZE;

        public AesCtrHmacContentCrypt(SecretKey macKey)
        {
            _macKey = macKey;
        }

        /// <inheritdoc/>
        public override void EncryptChunk(ReadOnlySpan<byte> cleartextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk)
        {
            // Chunk nonce
            secureRandom.GetBytes(ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE));

            // Encrypt
            AesCtr128.Encrypt(
                cleartextChunk,
                header.GetHeaderContentKey(),
                ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE),
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE, cleartextChunk.Length));

            // Calculate MAC
            CalculateChunkMac(
                header.GetHeaderNonce(),
                ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE),
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE, cleartextChunk.Length),
                chunkNumber,
                ciphertextChunk.Slice(CHUNK_NONCE_SIZE + cleartextChunk.Length, CHUNK_MAC_SIZE));
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber,
            ReadOnlySpan<byte> header, Span<byte> cleartextChunk)
        {
            // Allocate byte* for MAC
            Span<byte> mac = stackalloc byte[CHUNK_MAC_SIZE];

            // Calculate MAC
            CalculateChunkMac(
                header.GetHeaderNonce(),
                ciphertextChunk.GetChunkNonce(),
                ciphertextChunk.GetChunkPayload(),
                chunkNumber,
                mac);

            // Check MAC
            if (!mac.SequenceEqual(ciphertextChunk.GetChunkMac()))
                return false;

            // Decrypt
            AesCtr128.Decrypt(
                ciphertextChunk.GetChunkPayload(),
                header.GetHeaderContentKey(),
                ciphertextChunk.GetChunkNonce(),
                cleartextChunk);

            return true;
        }

        [SkipLocalsInit]
        private void CalculateChunkMac(ReadOnlySpan<byte> headerNonce, ReadOnlySpan<byte> chunkNonce, ReadOnlySpan<byte> ciphertextPayload, long chunkNumber, Span<byte> chunkMac)
        {
            // Convert long to byte array
            Span<byte> beChunkNumber = stackalloc byte[sizeof(long)];
            Unsafe.As<byte, long>(ref beChunkNumber[0]) = chunkNumber;

            // Reverse byte order as needed
            if (BitConverter.IsLittleEndian)
                beChunkNumber.Reverse();

            // Initialize HMAC
            using var hmacSha256 = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, _macKey);

            hmacSha256.AppendData(headerNonce);         // headerNonce
            hmacSha256.AppendData(beChunkNumber);       // beChunkNumber
            hmacSha256.AppendData(chunkNonce);          // chunkNonce
            hmacSha256.AppendData(ciphertextPayload);   // ciphertextPayload   

            hmacSha256.GetCurrentHash(chunkMac);
        }
    }
}
