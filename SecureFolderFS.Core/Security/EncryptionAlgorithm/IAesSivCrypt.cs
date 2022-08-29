namespace SecureFolderFS.Core.Security.EncryptionAlgorithm
{
    public interface IAesSivCrypt
    {
        byte[] AesSivEncrypt(byte[] cleartextBytes, byte[] encryptionKey, byte[] macKey, byte[] associatedData);

        byte[] AesSivDecrypt(byte[] ciphertextBytes, byte[] encryptionKey, byte[] macKey, byte[] associatedData);
    }
}
