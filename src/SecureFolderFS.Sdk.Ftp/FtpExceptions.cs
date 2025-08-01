namespace SecureFolderFS.Sdk.Ftp
{
    public static class FtpExceptions
    {
        public static Exception NotConnectedException { get; } = new("FTP connection is not established.");
    }
}