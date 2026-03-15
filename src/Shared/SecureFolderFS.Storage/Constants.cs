namespace SecureFolderFS.Storage
{
    public class Constants
    {
        public static class Sizes
        {
            public const long KILOBYTE = 1024;
            public const long MEGABYTE = KILOBYTE * 1024;
            public const long GIGABYTE = MEGABYTE * 1024;
            public const long TERABYTE = GIGABYTE * 1024;
        }

        public static class SpecialNames
        {
            public static string[] IllegalNames { get; } =
            [
                // Windows
                "con", "prn", "aux", "nul",
                "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",

                // Unix
                ".", "..",

                // MacOS
                ".DS_Store"
            ];
        }
    }
}
