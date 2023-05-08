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

        public static class VaultContent
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
