namespace SecureFolderFS.Sdk
{
    public static class Constants
    {
        public static class Widgets
        {
            public const string HEALTH_WIDGET_ID = "health_widget";
            public const string GRAPHS_WIDGET_ID = "graphs_widget";
            public const string AGGREGATED_DATA_WIDGET_ID = "aggregatedStatistics_widget";

            public static class Graphs
            {
                public const int MAX_GRAPH_POINTS = 30;
                public const int GRAPH_UPDATE_INTERVAL_MS = 200; // 0.2s
                public const int GRAPH_REFRESH_RATE = 1000 / GRAPH_UPDATE_INTERVAL_MS;
            }

            public static class Health
            {
                public const bool ARE_UPDATES_OPTIMIZED = true;
                public const bool IS_SCANNING_PARALLELIZED = false;
                public const double INTERVAL_MULTIPLIER = 0.2d;
            }
        }

        public static class Dialogs
        {
            public const int EXPLANATION_DIALOG_TIME_TICKS = 8;
        }

        public static class Vault
        {
            public const string VAULT_ICON_FILENAME = "vault.icon";
            public const int MAX_FREE_AMOUNT_OF_VAULTS = 2;
            public const string VAULT_README_FILENAME = "_readme_before_continuing.txt";
            public const string VAULT_README_MESSAGE = """
                                                              PLEASE READ BEFORE USING THIS VAULT

                This is the vault folder where important configuration files and your documents in their encrypted form are stored.
                Do not add/remove/modify any files or folders, as doing so may corrupt your vault.
                Any files manually added to this folder or the 'content' folder will NOT be encrypted regardless of whether the vault is unlocked or not.

                To access and securely store your files, first unlock the vault in SecureFolderFS, and then click the 'View vault' button.
                This will open a virtual storage directory where the files you add will be automatically encrypted on the hard drive.
                """;
        }

        public static class IntegrationPermissions
        {
            public const string ENUMERATE_VAULTS = "enumerate_vaults"; // List all added vaults and get basic info
            public const string VAULT_STATE_EVENTS = "vault_state_events"; // Listen to vault state events like unlocking or locking
            public const string RESCAP_MANAGE_VAULTS = "manage_vaults"; // Add existing or remove vaults from saved list
            public const string RESCAP_FILESYSTEM_PROVIDER = "filesystem_provider"; // Ability to provide custom file systems
            public const string RESCAP_AUTHENTICATION_PROVIDER = "authentication_provider"; // Ability to provide custom authentication for vaults
        }
    }
}
