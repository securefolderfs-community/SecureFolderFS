using System.Linq;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Exceptions;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    internal sealed class AesCtrHmacHeaderCryptor : BaseHeaderCryptor<AesCtrHmacFileHeader>
    {
        public override int HeaderSize { get; } = AesCtrHmacFileHeader.HEADER_SIZE;

        public AesCtrHmacHeaderCryptor(MasterKey masterKey, ICipherProvider keyCryptor)
            : base(masterKey, keyCryptor)
        {
        }

        public override IFileHeader CreateFileHeader()
        {
            AssertNotDisposed();

            var nonce = new byte[AesCtrHmacFileHeader.HEADER_NONCE_SIZE];
            var contentKey = new byte[AesCtrHmacFileHeader.HEADER_CONTENTKEY_SIZE];

            secureRandom.GetNonZeroBytes(nonce);
            secureRandom.GetNonZeroBytes(contentKey);

            return new AesCtrHmacFileHeader(nonce, contentKey);
        }

        public override byte[] EncryptHeader(AesCtrHmacFileHeader fileHeader)
        {
            var encKey = masterKey.GetEncryptionKey();
            var macKey = masterKey.GetMacKey();

            // Payload
            var ciphertextPayload = cipherProvider.AesCtrCrypt.AesCtrEncrypt(fileHeader.ContentKey, encKey, fileHeader.Nonce);

            // Mac
            var mac = CalculateFileHeaderMac(macKey, fileHeader.Nonce, ciphertextPayload);

            return AesCtrHmacFileHeader.ConstructCiphertextFileHeader(fileHeader.Nonce, ciphertextPayload, mac);
        }

        public override IFileHeader DecryptHeader(byte[] ciphertextFileHeader)
        {
            AssertNotDisposed();

            var encKey = masterKey.GetEncryptionKey();
            var macKey = masterKey.GetMacKey();

            var nonce = ciphertextFileHeader.SliceArray(0, AesCtrHmacFileHeader.HEADER_NONCE_SIZE);
            var ciphertextPayload = ciphertextFileHeader.SliceArray(AesCtrHmacFileHeader.HEADER_NONCE_SIZE, AesCtrHmacFileHeader.HEADER_CONTENTKEY_SIZE);
            var mac = ciphertextFileHeader.SliceArray(AesCtrHmacFileHeader.HEADER_NONCE_SIZE + AesCtrHmacFileHeader.HEADER_CONTENTKEY_SIZE, AesCtrHmacFileHeader.HEADER_MAC_SIZE);

            var realMac = CalculateFileHeaderMac(macKey, nonce, ciphertextPayload);
            if (!realMac.SequenceEqual(mac))
            {
                throw UnauthenticFileHeaderException.ForAesCtrHmac();
            }

            var cleartextPayload = cipherProvider.AesCtrCrypt.AesCtrDecrypt(ciphertextPayload, encKey, nonce);
            return new AesCtrHmacFileHeader(nonce, cleartextPayload);
        }

        private byte[] CalculateFileHeaderMac(SecretKey macKey, byte[] fileHeaderNonce, byte[] ciphertextPayload)
        {
            using var hmacSha256Crypt = cipherProvider.HmacSha256Crypt.GetInstance();
            hmacSha256Crypt.InitializeHMAC(macKey);
            hmacSha256Crypt.Update(fileHeaderNonce);
            hmacSha256Crypt.DoFinal(ciphertextPayload);

            return hmacSha256Crypt.GetHash();
        }
    }
}
