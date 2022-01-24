using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IXChaCha20Poly1305Crypt : IDisposable
    {
        byte[] XChaCha20Poly1305Encrypt(byte[] bytes, byte[] key, byte[] nonce, out byte[] tag, byte[] associatedData = null);

        byte[] XChaCha20Poly1305Decrypt(byte[] bytes, byte[] key, byte[] nonce, byte[] tag, byte[] associatedData = null);
    }
}
