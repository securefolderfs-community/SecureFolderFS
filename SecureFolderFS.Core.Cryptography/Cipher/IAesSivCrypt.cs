using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IAesSivCrypt : IDisposable
    {
        byte[] Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData);

        byte[]? Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> encKey, ReadOnlySpan<byte> macKey, ReadOnlySpan<byte> associatedData);
    }
}
