namespace SecureFolderFS.UI
{
    public static class Constants
    {
        public const string MAIN_WINDOW_ID = "SecureFolderFS_mainwindow";
        public const string MAIN_INSTANCE_ID = "SecureFolderFS_maininstance";
        public const string DATA_CONTAINER_ID = "SecureFolderFS_datacontainer";
        public const string STORABLE_BOOKMARK_RID = "bookmark:";
        public const string AUTOSTART_ARGUMENT = "--autostart";

        public static class GitHub
        {
            public const string REPOSITORY_NAME = "SecureFolderFS";
            public const string REPOSITORY_OWNER = "securefolderfs-community";
        }

        public static class FileNames
        {
            public const string KEY_FILE_EXTENSION = ".key";
            public const string VAULT_SHORTCUT_FILE_EXTENSION = ".sfvault";
            public const string ICON_ASSET_PATH = "Assets/AppAssets/app_icon.ico";

            public static class Accounts
            {
                public const string ACCOUNTS_FOLDER_NAME = "accounts";
                public const string ACCOUNT_DEVICE_KEY_FILENAME = "device_key.dat";
                public const string ACCOUNT_DEVICE_ID_FILENAME = "device_id.dat";
                public const string ACCOUNT_METADATA_FILENAME = "account_metadata.dat";
                public const string ACCOUNT_CLIENT_DEVICE_ID_FILENAME = "client_device_id.dat";
            }

            public static class Settings
            {
                public const string VAULTS_WIDGETS_FOLDERNAME = "vaults_widgets";
                public const string SETTINGS_FOLDER_NAME = "settings";
                public const string APPLICATION_SETTINGS_FILENAME = "application_settings.json";
                public const string SAVED_VAULTS_FILENAME = "saved_vaults.json";
                public const string USER_SETTINGS_FILENAME = "user_settings.json";
            }
        }

        public static class AppThemes
        {
            public const string LIGHT_THEME = "LIGHT";
            public const string DARK_THEME = "DARK";
            public const string THEME_PREFERENCE_SETTING = "ImmersiveTheme";
        }

        public static class Application
        {
            public const string EXCEPTION_BLOCK_DATE_FORMAT = "dd.MM.yyyy HH_mm_ss";
            public const string EXCEPTION_LOG_FILENAME = "securefolderfs_exceptionlog.log";
            public const string SESSION_EXCEPTION_FILENAME = "securefolderfs_sessionexception.log";
            public const string DEFAULT_CULTURE_STRING = "en-US";
        }

        public static class Browser
        {
            public const int THUMBNAIL_MAX_PARALLELISATION = 4;
            public const int IMAGE_THUMBNAIL_MAX_SIZE = 296;
            public const int IMAGE_THUMBNAIL_QUALITY = 75;
        }

        public static class FileData
        {
            public const string DESKTOP_INI_ICON_CONFIGURATION = """
                                                                 [.ShellClassInfo]
                                                                 IconResource={0},0
                                                                 InfoTip={1}
                                                                 [ViewState]
                                                                 FolderType = Generic
                                                                 """;
        }
    }
}
