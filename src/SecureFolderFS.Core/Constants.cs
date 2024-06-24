using System;

namespace SecureFolderFS.Core
{
    public static class Constants
    {
        public const string KEY_TEXT_SEPARATOR = "@@@";

        public static class Vault
        {
            public static class Names
            {
                public const string VAULT_CONTENT_FOLDERNAME = "content";
                public const string VAULT_CONFIGURATION_FILENAME = "sfconfig.cfg";
                public const string VAULT_AUTHENTICATION_FILENAME = "sfauth.cfg";
                public const string VAULT_KEYSTORE_FILENAME = "keystore.cfg";
            }

            public static class AuthenticationMethods
            {
                public const char SEPARATOR = ';';
                public const string AUTH_NONE = "None";
                public const string AUTH_PASSWORD = "Password";
                public const string AUTH_KEYFILE = "KeyFile";
                public const string AUTH_WINDOWS_HELLO = "WindowsHello";
                public const string AUTH_HARDWARE_KEY = "HardwareKey";
                public const string AUTH_APPLE_FACEID = "AppleFaceID";
                public const string AUTH_APPLE_TOUCHID = "AppleTouchID";
                public const string AUTH_ANDROID_BIOMETRIC = "AndroidBiometric";
            }

            [Obsolete]
            public static class Specializations
            {
                public const string SPEC_STANDARD = "Standard";
                public const string SPEC_JOURNAL = "Journal";
                public const string SPEC_GALLERY = "Gallery";
                public const string SPEC_MUSIC = "Music";
                public const string SPEC_NOTES = "Notes";
            }

            public static class Associations
            {
                public const string ASSOC_CONTENT_CIPHER_ID = "contentCipherScheme";
                public const string ASSOC_FILENAME_CIPHER_ID = "filenameCipherScheme";
                public const string ASSOC_SPECIALIZATION = "spec";
                public const string ASSOC_AUTHENTICATION = "authMode";
                public const string ASSOC_VAULT_ID = "vaultId";
                public const string ASSOC_VERSION = "version";
            }

            public static class Versions
            {
                public const int V1 = 1;
                public const int V2 = 2;
                public const int LATEST_VERSION = V2;
            }
        }

        public static class FileSystemId
        {
            public const string FS_DOKAN = "DOKANY";
            public const string FS_FUSE = "FUSE";
            public const string FS_WEBDAV = "WEBDAV";
        }
    }
}
