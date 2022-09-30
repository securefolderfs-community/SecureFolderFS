using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IArgon2idCrypt
    {
        void DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result);
    }
}
