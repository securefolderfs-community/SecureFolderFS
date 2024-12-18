using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    /// TODO: Needs docs
    public static class AesGcm128
    {
        public static void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            using var aesGcm = new AesGcm(key, Constants.Crypto.Chunks.AesGcm.CHUNK_TAG_SIZE);
            aesGcm.Encrypt(nonce, bytes, result, tag, associatedData);
        }

        public static void Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            using var aesGcm = new AesGcm(key, Constants.Crypto.Chunks.AesGcm.CHUNK_TAG_SIZE);
            aesGcm.Decrypt(nonce, bytes, tag, result, associatedData);
        }

        public static bool TryDecrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            try
            {
                Decrypt(bytes, key, nonce, tag, result, associatedData);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
