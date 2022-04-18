using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IAesCtrCrypt : IDisposable
    {
        byte[] AesCtrEncrypt(byte[] bytes, byte[] key, byte[] iv);

        byte[] AesCtrEncrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv);

        byte[] AesCtrDecrypt(byte[] bytes, byte[] key, byte[] iv);

        byte[] AesCtrDecrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv);
    }
}
