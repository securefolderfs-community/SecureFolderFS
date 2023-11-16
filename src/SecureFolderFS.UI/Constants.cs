namespace SecureFolderFS.UI
{
    public static class Constants
    {
        public const string MAIN_WINDOW_ID = "SecureFolderFS_mainwindow";

        public static class GitHub
        {
            public const string REPOSITORY_NAME = "SecureFolderFS";
            public const string REPOSITORY_OWNER = "securefolderfs-community";
        }

        public static class LocalSettings
        {
            public const string VAULTS_WIDGETS_FOLDERNAME = "vaults_widgets";
            public const string SETTINGS_FOLDER_NAME = "settings";
            public const string APPLICATION_SETTINGS_FILENAME = "application_settings.json";
            public const string SAVED_VAULTS_FILENAME = "saved_vaults.json";
            public const string USER_SETTINGS_FILENAME = "user_settings.json";
        }

        public static class AppThemes
        {
            public const string LIGHT_THEME = "LIGHT";
            public const string DARK_THEME = "DARK";
        }

        public static class AppLocalSettings
        {
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
            public const string VAULT_README_MESSAGE = "\n\t\t\t\tIMPORTATNT INFORMATION BEFORE USE" +
                                                       "\n" +
                                                       "\nThis is the root folder where necessary configuration data and all files in their encrypted form are stored." +
                                                       "\nDo not remove or modify any files or folders, nor add any new items, as doing so may corrupt your vault." +
                                                       "\nAny files manually added to this folder will not be encrypted by SecureFolderFS." +
                                                       "\n" +
                                                       "\nTo ensure that your files are stored securely, first unlock the vault in SecureFolderFS app," +
                                                       "\nand then click the \"View vault\" button. This will open the folder where you can place your files," +
                                                       "\nwhich will be automatically encrypted.";
        }
    }
}
