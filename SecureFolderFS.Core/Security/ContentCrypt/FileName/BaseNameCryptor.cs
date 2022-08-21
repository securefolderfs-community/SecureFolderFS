using System;
using System.Text;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileName
{
    internal abstract class BaseNameCryptor : IFileNameCryptor
    {
        protected readonly ICipherProvider keyCryptor;

        protected readonly MasterKey masterKey;

        private bool _disposed;

        protected BaseNameCryptor(ICipherProvider keyCryptor, MasterKey masterKey)
        {
            this.keyCryptor = keyCryptor;
            this.masterKey = masterKey;
        }

        public virtual string EncryptFileName(string cleartextName, DirectoryId directoryId)
        {
            AssertNotDisposed();

            var encKey = masterKey.GetEncryptionKey();
            var macKey = masterKey.GetMacKey();

            var cleartextFileNameBuffer = Encoding.UTF8.GetBytes(cleartextName);
            var ciphertextFileNameBuffer = EncryptFileName(cleartextFileNameBuffer, encKey, macKey, directoryId);

            return EncodingHelpers.WithBase64UrlEncoding(Convert.ToBase64String(ciphertextFileNameBuffer));
        }

        public virtual string DecryptFileName(string ciphertextFileName, DirectoryId directoryId)
        {
            AssertNotDisposed();

            var encKey = masterKey.GetEncryptionKey();
            var macKey = masterKey.GetMacKey();

            var ciphertextFileNameBuffer = Convert.FromBase64String(EncodingHelpers.WithoutBase64UrlEncoding(ciphertextFileName));
            var cleartextFileNameBuffer = DecryptFileName(ciphertextFileNameBuffer, encKey, macKey, directoryId);

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
