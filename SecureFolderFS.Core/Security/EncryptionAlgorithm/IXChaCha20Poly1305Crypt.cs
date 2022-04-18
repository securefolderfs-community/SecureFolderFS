using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IXChaCha20Poly1305Crypt : IDisposable
    {
        byte[] XChaCha20Poly1305Encrypt(byte[] bytes, byte[] key, byte[] nonce, out byte[] tag, byte[] associatedData = null);

        void XChaCha20Poly1305Encrypt2(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> resultAndtag, ReadOnlySpan<byte> associatedData = default);

        byte[] XChaCha20Poly1305Decrypt(byte[] bytes, byte[] key, byte[] nonce, byte[] tag, byte[] associatedData = null);

        bool XChaCha20Poly1305Decrypt2(ReadOnlySpan<byte> bytesWithTag, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> result, ReadOnlySpan<byte> associatedData = default);
    }
}
