using System;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IArgon2idCrypt : IDisposable
    {
        byte[] Argon2idHash(byte[] password, byte[] salt);

        bool VerifyArgon2idHash(byte[] origin, byte[] password, byte[] salt);
    }
}
