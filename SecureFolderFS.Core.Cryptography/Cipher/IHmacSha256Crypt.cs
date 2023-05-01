using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IHmacSha256Crypt
    {
        int MacSize { get; }

        IHmacSha256Instance GetInstance();
    }

    public interface IHmacSha256Instance : IDisposable
    {
        void InitializeHmac(ReadOnlySpan<byte> key);

        void Update(ReadOnlySpan<byte> bytes);

        void GetHash(Span<byte> destination);
    }
}
