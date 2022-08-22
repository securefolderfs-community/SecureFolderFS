using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IAesCtrCrypt
    {
        void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result);

        bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> result);
    }
}
