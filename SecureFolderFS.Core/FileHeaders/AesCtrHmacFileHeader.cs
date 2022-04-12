using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.FileHeaders
{
    internal sealed class AesCtrHmacFileHeader : BaseFileHeader
    {
        public const int HEADER_NONCE_SIZE = 16;

        public const int HEADER_CONTENTKEY_SIZE = 32;

        public const int HEADER_MAC_SIZE = 32;

        public const int HEADER_SIZE = HEADER_NONCE_SIZE + HEADER_CONTENTKEY_SIZE + HEADER_MAC_SIZE;

        public AesCtrHmacFileHeader(byte[] nonce, byte[] contentKey)
            : base(nonce, contentKey)
        {
        }

        public static byte[] ConstructCiphertextFileHeader(byte[] headerNonce, byte[] ciphertextPayload, byte[] headerMac)
        {
            var constructed = new byte[HEADER_SIZE];
            constructed.EmplaceArrays(headerNonce, ciphertextPayload, headerMac);

            return constructed;
        }
    }
}
