using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IAesCtrCrypt
    {
        void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result);

        bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result);
    }
}
