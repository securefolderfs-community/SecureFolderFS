namespace SecureFolderFS.UI
{
    public static class Constants
    {
        public const string MAIN_WINDOW_ID = "SecureFolderFS_mainwindow";
        public const string STORABLE_BOOKMARK_RID = "bookmark:";

        public static class GitHub
        {
            public const string REPOSITORY_NAME = "SecureFolderFS";
            public const string REPOSITORY_OWNER = "securefolderfs-community";
        }

        public static class FileNames
        {
            public const string KEY_FILE_EXTENSION = ".key";
            public const string VAULTS_WIDGETS_FOLDERNAME = "vaults_widgets";
            public const string SETTINGS_FOLDER_NAME = "settings";
            public const string APPLICATION_SETTINGS_FILENAME = "application_settings.json";
            public const string SAVED_VAULTS_FILENAME = "saved_vaults.json";
            public const string USER_SETTINGS_FILENAME = "user_settings.json";
            public const string ICON_ASSET_PATH = "Assets/AppAssets/AppIcon.ico";
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
            public const string DEFAULT_CULTURE_STRING = "en-US";
        }

        public static class Vault
        {
            public const string VAULT_README_FILENAME = "_readme_before_continuing.txt";
            public const string VAULT_README_MESSAGE = "\n\t\t\t\tIMPORTANT INFORMATION. READ CAREFULLY" +
                                                       "\n" +
                                                       "\nThis is the root folder where necessary configuration data and all files in their encrypted form are stored." +
                                                       "\nDo not remove or modify any of the configuration information, as doing so may corrupt your vault." +
                                                       "\nSecureFolderFS will not encrypt any documents manually added to this folder" +
                                                       "\n" +
                                                       "\nTo ensure that your files are stored securely, first unlock the vault in the SecureFolderFS app," +
                                                       "\nand click the \"View vault\" button. This will open the folder where any files you add" +
                                                       "\nwill be automatically encrypted.";
        }
    }
}
