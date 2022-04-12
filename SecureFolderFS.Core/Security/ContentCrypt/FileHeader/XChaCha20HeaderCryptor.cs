using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    internal sealed class XChaCha20HeaderCryptor : BaseHeaderCryptor<XChaCha20FileHeader>
    {
        public override int HeaderSize { get; } = XChaCha20FileHeader.HEADER_SIZE;

        public XChaCha20HeaderCryptor(MasterKey masterKey, IKeyCryptor keyCryptor)
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
            using SecretKey encKey = masterKey.CreateEncryptionKeyCopy();

            // Payload
            var ciphertextPayload = keyCryptor.XChaCha20Poly1305Crypt.XChaCha20Poly1305Encrypt(fileHeader.ContentKey, encKey, fileHeader.Nonce, out byte[] tag);

            return XChaCha20FileHeader.ConstructCiphertextFileHeader(fileHeader.Nonce, ciphertextPayload, tag);
        }

        public override IFileHeader DecryptHeader(byte[] ciphertextFileHeader)
        {
            AssertNotDisposed();

            using SecretKey encKey = masterKey.CreateEncryptionKeyCopy();

            var nonce = ciphertextFileHeader.Slice(0, XChaCha20FileHeader.HEADER_NONCE_SIZE);
            var ciphertextPayload = ciphertextFileHeader.Slice(XChaCha20FileHeader.HEADER_NONCE_SIZE, XChaCha20FileHeader.HEADER_CONTENTKEY_SIZE);
            var tag = ciphertextFileHeader.Slice(XChaCha20FileHeader.HEADER_NONCE_SIZE + XChaCha20FileHeader.HEADER_CONTENTKEY_SIZE, XChaCha20FileHeader.HEADER_TAG_SIZE);

            var cleartextPayload = keyCryptor.XChaCha20Poly1305Crypt.XChaCha20Poly1305Decrypt(ciphertextPayload, encKey, nonce, tag);
            if (cleartextPayload == null)
            {
                throw UnauthenticFileHeaderException.ForXChaCha20();
            }

            return new XChaCha20FileHeader(nonce, cleartextPayload);
        }
    }
}
