using Miscreant;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class AesSivCrypt : IAesSivCrypt
    {
        // TODO: Check correctness of passed parameters

        public byte[] AesSivEncrypt(byte[] cleartextBytes, byte[] encryptionKey, byte[] macKey, byte[] associatedData)
        {
            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey, macKey);

            // The longKey will be split into two keys - one for S2V and the other one for CTR

            using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            return aesCmacSiv.Seal(cleartextBytes, data: associatedData);
        }

        public byte[] AesSivDecrypt(byte[] ciphertextBytes, byte[] encryptionKey, byte[] macKey, byte[] associatedData)
        {
            var longKey = new byte[encryptionKey.Length + macKey.Length];
            longKey.EmplaceArrays(encryptionKey, macKey);

            // The longKey will be split into two keys - one for S2V and the other one for CTR

            using var aesCmacSiv = Aead.CreateAesCmacSiv(longKey);
            return aesCmacSiv.Open(ciphertextBytes, data: associatedData);
        }
    }
}
