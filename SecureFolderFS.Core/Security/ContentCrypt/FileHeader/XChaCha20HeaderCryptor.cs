using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    internal sealed class XChaCha20HeaderCryptor : BaseHeaderCryptor<XChaCha20FileHeader>
    {
        public override int HeaderSize { get; } = XChaCha20FileHeader.HEADER_SIZE;

        public XChaCha20HeaderCryptor(MasterKey masterKey, ICipherProvider keyCryptor)
            : base(masterKey, keyCryptor)
        {
        }

        public override IFileHeader CreateFileHeader()
        {
            AssertNotDisposed();

            var nonce = new byte[XChaCha20FileHeader.HEADER_NONCE_SIZE];
            var contentKey = new byte[XChaCha20FileHeader.HEADER_CONTENTKEY_SIZE];

            secureRandom.GetNonZeroBytes(nonce);
            secureRandom.GetNonZeroBytes(contentKey);

            return new XChaCha20FileHeader(nonce, contentKey);
        }

        public override byte[] EncryptHeader(XChaCha20FileHeader fileHeader)
        {
            var encKey = masterKey.GetEncryptionKey();

            // Payload
            var ciphertextPayload = cipherProvider.XChaCha20Poly1305Crypt.Encrypt(fileHeader.ContentKey, encKey, fileHeader.Nonce, out byte[] tag);

            return XChaCha20FileHeader.ConstructCiphertextFileHeader(fileHeader.Nonce, ciphertextPayload, tag);
        }

        public override IFileHeader DecryptHeader(byte[] ciphertextFileHeader)
        {
            AssertNotDisposed();

            var encKey = masterKey.GetEncryptionKey();

            var nonce = ciphertextFileHeader.SliceArray(0, XChaCha20FileHeader.HEADER_NONCE_SIZE);
            var ciphertextPayload = ciphertextFileHeader.SliceArray(XChaCha20FileHeader.HEADER_NONCE_SIZE, XChaCha20FileHeader.HEADER_CONTENTKEY_SIZE);
            var tag = ciphertextFileHeader.SliceArray(XChaCha20FileHeader.HEADER_NONCE_SIZE + XChaCha20FileHeader.HEADER_CONTENTKEY_SIZE, XChaCha20FileHeader.HEADER_TAG_SIZE);

            var cleartextPayload = cipherProvider.XChaCha20Poly1305Crypt.Decrypt(ciphertextPayload, encKey, nonce, tag);
            if (cleartextPayload is null)
            {
                throw UnauthenticFileHeaderException.ForXChaCha20();
            }

            return new XChaCha20FileHeader(nonce, cleartextPayload);
        }
    }
}
