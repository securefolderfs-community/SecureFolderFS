using System;
using System.Security.Cryptography;
using Miscreant;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class AesSivCrypt : IAesSivCrypt
    {
        // TODO: Check correctness of passed parameters

        public void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey,
            ReadOnlySpan<byte> associatedData, Span<byte> result)
        {
            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey.ToArray(), macKey.ToArray());

            // The longKey will be split into two keys - one for S2V and the other one for CTR

            using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            var result2 = aesCmacSiv.Seal(bytes.ToArray(), data: associatedData.ToArray());
            
            result2.CopyTo(result);
        }

        public bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey,
            ReadOnlySpan<byte> associatedData, Span<byte> result)
        {
            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey.ToArray(), macKey.ToArray());

            try
            {
                // The longKey will be split into two keys - one for S2V and the other one for CTR

                using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
                var result2 = aesCmacSiv.Open(bytes.ToArray(), data: associatedData.ToArray());

                result2.CopyTo(result);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
