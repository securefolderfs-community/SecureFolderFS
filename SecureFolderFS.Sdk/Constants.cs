namespace SecureFolderFS.Sdk
{
    public static class Constants
    {
        public static class Graphs
        {
            public const int MAX_GRAPH_POINTS = 30;
            public const int GRAPH_UPDATE_INTERVAL_MS = 200; // 0.2s
            public const int GRAPH_REFRESH_RATE = 1000 / GRAPH_UPDATE_INTERVAL_MS;
        }

        public static class Widgets
        {
            public const string HEALTH_WIDGET_ID = "health_widget";
            public const string GRAPHS_WIDGET_ID = "graphs_widget";
        }

        public static class Iap
        {
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
    }
}
