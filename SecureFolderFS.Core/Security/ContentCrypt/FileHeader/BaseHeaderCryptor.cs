using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Sdk.Streams;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    internal abstract class BaseHeaderCryptor<TFileHeader> : IFileHeaderCryptor
        where TFileHeader : class, IFileHeader
    {
        protected readonly MasterKey masterKey;

        protected readonly ICipherProvider cipherProvider;

        protected readonly RandomNumberGenerator secureRandom;

        private bool _disposed;

        public abstract int HeaderSize { get; }

        protected BaseHeaderCryptor(MasterKey masterKey, ICipherProvider cipherProvider)
        {
            this.masterKey = masterKey;
            this.cipherProvider = cipherProvider;
            this.secureRandom = RandomNumberGenerator.Create();
        }

        public byte[] EncryptHeader(IFileHeader fileHeader)
        {
            AssertNotDisposed();

            if (fileHeader is not TFileHeader requestedFileHeader)
            {
                throw ErrorHandlingHelpers.GetBadTypeException(nameof(fileHeader), typeof(TFileHeader));
            }

            return EncryptHeader(requestedFileHeader);
        }

        public byte[] CiphertextHeaderFromCiphertextFileStream(ICiphertextFileStream ciphertextFileStream)
        {
            var buffer = new byte[HeaderSize];
            var savedPosition = ciphertextFileStream.Position;

            ciphertextFileStream.Position = 0L;
            ciphertextFileStream.Read(buffer.AsSpan());
            ciphertextFileStream.Position = savedPosition;

            return buffer;
        }

        public abstract IFileHeader CreateFileHeader();

        public abstract byte[] EncryptHeader(TFileHeader fileHeader);

        public abstract IFileHeader DecryptHeader(byte[] ciphertextFileHeader);

        protected void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public virtual void Dispose()
        {
            _disposed = true;
            masterKey.Dispose();
            secureRandom.Dispose();
        }
    }
}
