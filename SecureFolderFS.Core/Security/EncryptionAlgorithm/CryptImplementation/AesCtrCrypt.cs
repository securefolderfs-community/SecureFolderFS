using System;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation.AesCtr;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class AesCtrCrypt : IAesCtrCrypt
    {
        public byte[] AesCtrEncrypt(byte[] bytes, byte[] key, byte[] iv)
        {
            ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

            using var aesCtr = new AesCounterMode(ulIv, 0UL);
            var encryptor = aesCtr.CreateEncryptor(key, null);

            return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public byte[] AesCtrDecrypt(byte[] bytes, byte[] key, byte[] iv)
        {
            ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

            using var aesCtr = new AesCounterMode(ulIv, 0UL);
            var decryptor = aesCtr.CreateDecryptor(key, null);

            return decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public byte[] AesCtrEncrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv)
        {
            ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

            using var aesCtr = new AesCounterMode(ulIv, 0UL);
            var encryptor = aesCtr.CreateEncryptor(key.ToArray(), null);

            return encryptor.TransformFinalBlock(bytes.ToArray(), 0, bytes.Length);
        }

        public byte[] AesCtrDecrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv)
        {
            ulong ulIv = BitConverter.ToUInt64(iv); // TODO: ulIv good here?

            using var aesCtr = new AesCounterMode(ulIv, 0UL);
            var decryptor = aesCtr.CreateDecryptor(key.ToArray(), null);

            return decryptor.TransformFinalBlock(bytes.ToArray(), 0, bytes.Length);
        }
    }
}
