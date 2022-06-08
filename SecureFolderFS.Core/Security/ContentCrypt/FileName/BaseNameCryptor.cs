using System;
using System.Text;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileName
{
    internal abstract class BaseNameCryptor : IFileNameCryptor
    {
        protected readonly IKeyCryptor keyCryptor;

        protected readonly MasterKey masterKey;

        private bool _disposed;

        protected BaseNameCryptor(IKeyCryptor keyCryptor, MasterKey masterKey)
        {
            keyCryptor = keyCryptor;
            masterKey = masterKey;
        }

        public virtual string EncryptFileName(string cleartextName, DirectoryId directoryId)
        {
            AssertNotDisposed();

            using SecretKey encKey = masterKey.CreateEncryptionKeyCopy();
            using SecretKey macKey = masterKey.CreateMacKeyCopy();

            byte[] cleartextFileNameBuffer = Encoding.UTF8.GetBytes(cleartextName);
            byte[] ciphertextFileNameBuffer = EncryptFileName(cleartextFileNameBuffer, encKey, macKey, directoryId);

            return EncodingHelpers.WithBase64UrlEncoding(Convert.ToBase64String(ciphertextFileNameBuffer));
        }

        public virtual string DecryptFileName(string ciphertextFileName, DirectoryId directoryId)
        {
            AssertNotDisposed();

            using SecretKey encKey = masterKey.CreateEncryptionKeyCopy();
            using SecretKey macKey = masterKey.CreateMacKeyCopy();

            byte[] ciphertextFileNameBuffer = Convert.FromBase64String(EncodingHelpers.WithoutBase64UrlEncoding(ciphertextFileName));
            byte[] cleartextFileNameBuffer = DecryptFileName(ciphertextFileNameBuffer, encKey, macKey, directoryId);

            return Encoding.UTF8.GetString(cleartextFileNameBuffer);
        }

        protected void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        protected abstract byte[] EncryptFileName(byte[] cleartextFileNameBuffer, SecretKey encryptionKey, SecretKey macKey, DirectoryId directoryId);

        protected abstract byte[] DecryptFileName(byte[] ciphertextFileNameBuffer, SecretKey encryptionKey, SecretKey macKey, DirectoryId directoryId);

        public virtual void Dispose()
        {
            _disposed = true;
            masterKey.Dispose();
        }
    }
}
