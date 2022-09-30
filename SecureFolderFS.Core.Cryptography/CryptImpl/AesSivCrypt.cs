using Miscreant;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.CryptImpl
{
    /// <inheritdoc cref="IAesSivCrypt"/>
    public sealed class AesSivCrypt : IAesSivCrypt
    {
        /// <inheritdoc/>
        public byte[] Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData)
        {
            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey.ToArray(), macKey.ToArray());

            // The longKey will be split into two keys - one for S2V and the other one for CTR

            using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            return aesCmacSiv.Seal(bytes.ToArray(), data: associatedData.ToArray());
        }

        /// <inheritdoc/>
        public byte[]? Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData)
        {
            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey.ToArray(), macKey.ToArray());

            try
            {
                // The longKey will be split into two keys - one for S2V and the other one for CTR

                using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
                return aesCmacSiv.Open(bytes.ToArray(), data: associatedData.ToArray());
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
    }
}
