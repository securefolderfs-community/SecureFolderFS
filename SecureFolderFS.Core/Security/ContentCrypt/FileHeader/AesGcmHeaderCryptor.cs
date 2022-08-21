using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Shared.Extensions;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    internal sealed class AesGcmHeaderCryptor : BaseHeaderCryptor<AesGcmFileHeader>
    {
        public override int HeaderSize { get; } = AesGcmFileHeader.HEADER_SIZE;

        public AesGcmHeaderCryptor(MasterKey masterKey, ICipherProvider cipherProvider)
            : base(masterKey, cipherProvider)
        {
        }

        public override IFileHeader CreateFileHeader()
        {
            AssertNotDisposed();

            var nonce = new byte[AesGcmFileHeader.HEADER_NONCE_SIZE];
            var contentKey = new byte[AesGcmFileHeader.HEADER_CONTENTKEY_SIZE];

            secureRandom.GetNonZeroBytes(nonce);
            secureRandom.GetNonZeroBytes(contentKey);

            return new AesGcmFileHeader(nonce, contentKey);
        }

        public override byte[] EncryptHeader(AesGcmFileHeader fileHeader)
        {
            var encKey = masterKey.GetEncryptionKey();

            // Payload
            var ciphertextPayload = cipherProvider.AesGcmCrypt.AesGcmEncryptDeprecated(fileHeader.ContentKey, encKey, fileHeader.Nonce, out byte[] tag);

            return AesGcmFileHeader.ConstructCiphertextFileHeader(fileHeader.Nonce, ciphertextPayload, tag);
        }

        public override IFileHeader DecryptHeader(byte[] ciphertextFileHeader)
        {
            AssertNotDisposed();

            var encKey = masterKey.GetEncryptionKey();

            try
            {
                var nonce = ciphertextFileHeader.Slice(0, AesGcmFileHeader.HEADER_NONCE_SIZE);
                var ciphertextPayload = ciphertextFileHeader.Slice(AesGcmFileHeader.HEADER_NONCE_SIZE, AesGcmFileHeader.HEADER_CONTENTKEY_SIZE);
                var tag = ciphertextFileHeader.Slice(AesGcmFileHeader.HEADER_NONCE_SIZE + AesGcmFileHeader.HEADER_CONTENTKEY_SIZE, AesGcmFileHeader.HEADER_TAG_SIZE);

                var cleartextPayload = cipherProvider.AesGcmCrypt.AesGcmDecryptDeprecated(ciphertextPayload, encKey, nonce, tag);

                return new AesGcmFileHeader(nonce, cleartextPayload);
            }
            catch (CryptographicException)
            {
                throw UnauthenticFileHeaderException.ForAesGcm();
            }
        }
    }
}
