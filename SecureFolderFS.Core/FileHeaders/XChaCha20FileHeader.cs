using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.FileHeaders
{
    internal sealed class XChaCha20FileHeader : BaseFileHeader
    {
        public const int HEADER_NONCE_SIZE = 24;

        public const int HEADER_CONTENTKEY_SIZE = 32;

        public const int HEADER_TAG_SIZE = 16;

        public const int HEADER_SIZE = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE + HEADER_TAG_SIZE;

        public XChaCha20FileHeader(byte[] nonce, byte[] contentKey)
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
