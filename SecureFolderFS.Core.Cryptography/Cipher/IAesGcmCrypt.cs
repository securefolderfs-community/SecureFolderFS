using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IAesGcmCrypt
    {
        byte[] AesGcmEncryptDeprecated(byte[] bytes, byte[] key, byte[] iv, out byte[] tag, byte[] associatedData = null);

        byte[] AesGcmDecryptDeprecated(byte[] bytes, byte[] key, byte[] iv, byte[] tag, byte[] associatedData = null);

        void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData);

        bool Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData);
    }
}
