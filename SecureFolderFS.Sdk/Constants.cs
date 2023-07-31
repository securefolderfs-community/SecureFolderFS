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
        }
    }
}
