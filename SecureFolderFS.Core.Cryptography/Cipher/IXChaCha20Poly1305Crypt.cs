using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IXChaCha20Poly1305Crypt
    {
        void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> resultAndTag, ReadOnlySpan<byte> associatedData);

        bool Decrypt(ReadOnlySpan<byte> bytesWithTag, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> result, ReadOnlySpan<byte> associatedData);
    }
}
