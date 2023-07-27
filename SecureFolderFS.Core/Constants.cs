namespace SecureFolderFS.Core
{
    public static class Constants
    {
        public const string KEY_TEXT_SEPARATOR = "@@@";

        public static class Vault
        {
            public const string VAULT_CONTENT_FOLDERNAME = "content";
            public const string VAULT_CONFIGURATION_FILENAME = "sfconfig.cfg";
            public const string VAULT_AUTHENTICATION_FILENAME = "sfauth.cfg";
            public const string VAULT_KEYSTORE_FILENAME = "keystore.cfg";
            public const string ENCRYPTED_FILE_EXTENSION = ".sffs";
        }

        public static class FileSystemId
        {
            public const string DOKAN_ID = "DOKANY";
            public const string FUSE_ID = "FUSE";
            public const string WEBDAV_ID = "WEBDAV";
        }

        public static class CipherId
        {
            public const string NONE = ""; // Empty
            public const string AES_CTR_HMAC = "AES-CTR HMAC";
            public const string AES_GCM = "AES-GCM";
            public const string XCHACHA20_POLY1305 = "XChaCha20-Poly1305";
            public const string AES_SIV = "AES-SIV";
        }

        public static class KeyChains
        {
            public const int ENCKEY_LENGTH = 32;
            public const int MACKEY_LENGTH = 32;
            public const int SALT_LENGTH = 16;
        }

        public static class VaultVersion
        {
            public const int V1 = 1;
            public const int V2 = 2;
            public const int LATEST_VERSION = V2;
        }

        public static class AuthenticationMethods
        {
            public const string AUTH_NONE = "None";
            public const string AUTH_PASSWORD = "Password";
            public const string AUTH_KEYFILE = "KeyFile";
            public const string AUTH_WINDOWS_HELLO = "WindowsHello";
            public const string AUTH_HARDWARE_KEY = "HardwareKey";
        }
    }
}
