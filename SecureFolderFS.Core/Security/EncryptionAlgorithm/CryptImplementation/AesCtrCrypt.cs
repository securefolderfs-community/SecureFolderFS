using System;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation.AesCtr;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class AesCtrCrypt : IAesCtrCrypt
    {
        private const ulong CTR_START = 0UL;

        public void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result)
        {
            ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

            using var aesCtr = new AesCounterMode(ulIv, CTR_START);
            var encryptor = aesCtr.CreateEncryptor(key.ToArray(), null);

            var result2 = encryptor.TransformFinalBlock(bytes.ToArray(), 0, bytes.Length);

            result2.CopyTo(result);
        }

        public bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result)
        {
            try
            {
                ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

                using var aesCtr = new AesCounterMode(ulIv, CTR_START);
                var decryptor = aesCtr.CreateDecryptor(key.ToArray(), null);

                var result2 = decryptor.TransformFinalBlock(bytes.ToArray(), 0, bytes.Length);

                result2.CopyTo(result);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
