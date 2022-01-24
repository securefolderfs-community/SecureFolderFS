using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IHmacSha256Crypt : IDisposable
    {
        IHmacSha256Crypt GetInstance(byte[] key);

        void InitializeHMAC();

        void Update(byte[] bytes);

        void DoFinal(byte[] bytes);

        byte[] GetHash();
    }
}
