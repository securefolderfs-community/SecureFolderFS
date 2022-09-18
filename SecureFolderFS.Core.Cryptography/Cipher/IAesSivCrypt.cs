using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IAesSivCrypt
    {
        byte[] Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData);

        byte[]? Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encryptionKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData);
    }
}
