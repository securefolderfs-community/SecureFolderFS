using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IAesSivCrypt
    {
        void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData, Span<byte> result);

        bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData, Span<byte> result);
    }
}
