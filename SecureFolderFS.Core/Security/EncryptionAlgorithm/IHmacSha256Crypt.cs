using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IHmacSha256Crypt : IDisposable
    {
        IHmacSha256Crypt GetInstance();

        void InitializeHMAC(byte[] key);

        void Update(ReadOnlySpan<byte> bytes);

        void DoFinal(ReadOnlySpan<byte> bytes);

        byte[] GetHash();

        void GetHash(Span<byte> destination);
    }
}
