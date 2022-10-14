namespace SecureFolderFS.Core.Dokany
{
    public static class Constants
    {
        public const string DOKAN_DLL = "dokan2.dll";
        public const int DOKAN_VERSION = 205;
        public const int DOKAN_MAX_VERSION = 210;

        internal static class FileSystem
        {
            public const long INVALID_HANDLE = 0L;
            public const uint MAX_COMPONENT_LENGTH = 256;
            public const int MAX_DRIVE_INFO_CALLS_UNTIL_GIVEUP = 5;

            public const DokanNet.FileAccess DATA_ACCESS =
                                                          DokanNet.FileAccess.ReadData
                                                        | DokanNet.FileAccess.WriteData
                                                        | DokanNet.FileAccess.AppendData
                                                        | DokanNet.FileAccess.Execute
                                                        | DokanNet.FileAccess.GenericExecute
                                                        | DokanNet.FileAccess.GenericWrite
                                                        | DokanNet.FileAccess.GenericRead;
            public const DokanNet.FileAccess DATA_WRITE_ACCESS =
                                                          DokanNet.FileAccess.WriteData
                                                        | DokanNet.FileAccess.AppendData
                                                        | DokanNet.FileAccess.Delete
                                                        | DokanNet.FileAccess.GenericWrite;
        }
    }
}
