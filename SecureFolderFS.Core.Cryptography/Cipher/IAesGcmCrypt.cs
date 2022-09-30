using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IAesGcmCrypt
    {
        void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData);

        bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData);
    }
}
