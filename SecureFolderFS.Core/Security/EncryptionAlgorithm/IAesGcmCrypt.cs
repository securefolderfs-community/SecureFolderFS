using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IAesGcmCrypt : IDisposable
    {
        int AesGcmTagSize { get; }

        byte[] AesGcmEncrypt(byte[] bytes, byte[] key, byte[] iv, out byte[] tag, byte[] associatedData = null);

        byte[] AesGcmDecrypt(byte[] bytes, byte[] key, byte[] iv, byte[] tag, byte[] associatedData = null);
    }
}
