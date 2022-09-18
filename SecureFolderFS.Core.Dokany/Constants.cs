namespace SecureFolderFS.Core.Dokany
{
    public static class Constants
    {
        internal const string DOKAN_DLL = "dokan2.dll";

        internal static class FileSystem
        {
            public const long INVALID_HANDLE = 0L;
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
