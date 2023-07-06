using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.Cipher.Default
{
    /// <inheritdoc cref="IAesGcmCrypt"/>
    public sealed class AesGcmCrypt : IAesGcmCrypt
    {
        /// <inheritdoc/>
        public void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result,
            ReadOnlySpan<byte> associatedData)
        {
            using var aesGcm = new AesGcm(key);
            aesGcm.Encrypt(nonce, bytes, result, tag, associatedData);
        }

        /// <inheritdoc/>
        public bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag, Span<byte> result,
            ReadOnlySpan<byte> associatedData)
        {
            try
            {
                using var aesGcm = new AesGcm(key);
                aesGcm.Decrypt(nonce, bytes, tag, result, associatedData);

                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
