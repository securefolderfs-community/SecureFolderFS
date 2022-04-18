using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IAesGcmCrypt : IDisposable
    {
        int AesGcmTagSize { get; }

        byte[] AesGcmEncrypt(byte[] bytes, byte[] key, byte[] iv, out byte[] tag, byte[] associatedData = null);

        void AesGcmEncrypt2(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData = default);

        byte[] AesGcmDecrypt(byte[] bytes, byte[] key, byte[] iv, byte[] tag, byte[] associatedData = null);

        void AesGcmDecrypt2(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData = default);
    }
}
