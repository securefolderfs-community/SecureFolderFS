using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IHmacSha256Crypt : IDisposable
    {
        void InitializeHmac(ReadOnlySpan<byte> key);

        void Update(ReadOnlySpan<byte> bytes);

        void DoFinal(ReadOnlySpan<byte> bytes);

        void GetHash(Span<byte> destination);
    }
}
