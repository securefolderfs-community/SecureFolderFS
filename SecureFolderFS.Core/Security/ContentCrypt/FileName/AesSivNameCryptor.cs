using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileName
{
    internal sealed class AesSivNameCryptor : BaseNameCryptor
    {
        public AesSivNameCryptor(IKeyCryptor keyCryptor, MasterKey masterKey)
            : base(keyCryptor, masterKey)
        {
        }

        protected override byte[] EncryptFileName(byte[] cleartextFileNameBuffer, SecretKey encryptionKey, SecretKey macKey, DirectoryId directoryId)
        {
            return keyCryptor.AesSivCrypt.AesSivEncrypt(cleartextFileNameBuffer, encryptionKey, macKey, directoryId.Id);
        }

        protected override byte[] DecryptFileName(byte[] ciphertextFileNameBuffer, SecretKey encryptionKey, SecretKey macKey, DirectoryId directoryId)
        {
            return keyCryptor.AesSivCrypt.AesSivDecrypt(ciphertextFileNameBuffer, encryptionKey, macKey, directoryId.Id);
        }
    }
}
