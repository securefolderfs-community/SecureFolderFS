using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IAesCtrCrypt : IDisposable
    {
        byte[] AesCtrEncrypt(byte[] bytes, byte[] key, byte[] iv);

        byte[] AesCtrDecrypt(byte[] bytes, byte[] key, byte[] iv);
    }
}
