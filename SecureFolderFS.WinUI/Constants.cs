namespace SecureFolderFS.WinUI
{
    internal static class Constants
    {
        public const string MAIN_WINDOW_ID = "main_window";

        public static class LocalSettings
        {
            public const string SETTINGS_FOLDER_NAME = "settings";

            public const string APPLICATION_SETTINGS_FILENAME = "application_settings.json";

            public const string VAULTS_SETTINGS_FILENAME = "vaults_settings.json";

            public const string USER_SETTINGS_FILENAME = "user_settings.json";

            public const string SECRET_SETTINGS_FILENAME = "confidential_settings.json";
        }

        public static class Application
        {
            public const string EXCEPTION_BLOCK_DATE_FORMAT = "dd.MM.yyyy HH_mm_ss";

            public const string EXCEPTIONLOG_FILENAME = "securefolderfs_exceptionlog.log";
        }
    }
}
