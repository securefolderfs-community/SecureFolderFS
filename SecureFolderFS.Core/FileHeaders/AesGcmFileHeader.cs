using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.FileHeaders
{
    internal sealed class AesGcmFileHeader : BaseFileHeader
    {
        public const int HEADER_NONCE_SIZE = 12; // AesGcm.NonceByteSizes.MaxSize = 12

        public const int HEADER_CONTENTKEY_SIZE = 32;

        public const int HEADER_TAG_SIZE = Constants.Security.EncryptionAlgorithm.AesGcm.AES_GCM_TAG_SIZE;

        public const int HEADER_SIZE = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE + HEADER_TAG_SIZE;

        public AesGcmFileHeader(byte[] nonce, byte[] contentKey)
            : base(nonce, contentKey)
        {
        }

        public static byte[] ConstructCiphertextFileHeader(byte[] headerNonce, byte[] ciphertextPayload, byte[] headerTag)
        {
            var constructed = new byte[HEADER_SIZE];
            constructed.EmplaceArrays(headerNonce, ciphertextPayload, headerTag);

            return constructed;
        }
    }
}
