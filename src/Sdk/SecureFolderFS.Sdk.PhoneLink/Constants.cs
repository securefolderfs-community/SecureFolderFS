namespace SecureFolderFS.Sdk.PhoneLink
{
    public static class Constants
    {
        public static readonly byte[] DISCOVERY_MAGIC = "SFFS-PHONELINK"u8.ToArray();
        public const string SECRETS_KEY_PREFIX = "PhoneLink_Secret_";
        public const string DATA_SOURCE_PHONE_LINK = $"{nameof(SecureFolderFS)}.{nameof(PhoneLink)}";
        public const byte PROTOCOL_VERSION = 1;
        public const int CHALLENGE_VALIDITY_SECONDS = 30;
        public const int COMMUNICATION_PORT = 41235;
        public const int DISCOVERY_PORT = 41234;
        public const int DISCOVERY_TIMEOUT_MS = 2000;
        public const int CONNECTION_TIMEOUT_MS = 5000;

        public static class KeyTraits
        {
            public const int MESSAGE_BYTE_LENGTH = 1;
            public const int NONCE_SIZE = 12;
            public const int TAG_SIZE = 16;
        }
    }
}
