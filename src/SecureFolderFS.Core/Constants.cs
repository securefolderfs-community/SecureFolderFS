using System;

namespace SecureFolderFS.Core
{
    public static class Constants
    {
        public static class Vault
        {
            public static class Names
            {
                public const string CONFIGURATION_EXTENSION = ".cfg";
                public const string VAULT_CONTENT_FOLDERNAME = "content";
                public const string VAULT_KEYSTORE_FILENAME = $"keystore{CONFIGURATION_EXTENSION}";
                public const string VAULT_CONFIGURATION_FILENAME = $"sfconfig{CONFIGURATION_EXTENSION}";
            }

            public static class Authentication
            {
                public const char SEPARATOR = ';';
                public const string AUTH_NONE = "none";
                public const string AUTH_PASSWORD = "password";
                public const string AUTH_KEYFILE = "key_file";
                public const string AUTH_WINDOWS_HELLO = "windows_hello";
                public const string AUTH_HARDWARE_KEY = "hardware_key";
                public const string AUTH_APPLE_FACEID = "apple_faceid";
                public const string AUTH_APPLE_TOUCHID = "apple_touchid";
                public const string AUTH_ANDROID_BIOMETRIC = "android_biometric";

                public const string AUTH_DEVICE_PING = "device_ping";
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
                public const string ASSOC_FILENAME_ENCODING_ID = "filenameEncoding";
                public const string ASSOC_RECYCLE_SIZE = "recycleBinSize";
                public const string ASSOC_SPECIALIZATION = "spec";
                public const string ASSOC_AUTHENTICATION = "authMode";
                public const string ASSOC_VAULT_ID = "vaultId";
                public const string ASSOC_VERSION = "version";
            }

            public static class Versions
            {
                public const int V1 = 1;
                public const int V2 = 2;
                public const int V3 = 3;
                public const int LATEST_VERSION = V3;
            }
        }

        public static class FileSystemId
        {
            public const string FS_FUSE = "FUSE";
            public const string FS_WEBDAV = "WEBDAV";
            public const string FS_ANDROID = "ANDROID_DOCUMENTS_PROVIDER";
            public const string FS_IOS = "IOS_FILE_PROVIDER";
            public const string FS_LOCAL = "ABSTRACT_STORAGE";
        }
    }
}
