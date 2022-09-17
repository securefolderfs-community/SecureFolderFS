namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public interface IArgon2idCrypt
    {
        byte[] Argon2idHash(byte[] password, byte[] salt);

        bool VerifyArgon2idHash(byte[] origin, byte[] password, byte[] salt);
    }
}
