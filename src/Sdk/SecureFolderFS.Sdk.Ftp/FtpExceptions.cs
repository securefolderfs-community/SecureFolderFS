using System;

namespace SecureFolderFS.Sdk.Ftp
{
    public static class FtpExceptions
    {
        /// <summary>
        /// Represents an exception thrown when an operation is attempted on an FTP client that is not connected to a server.
        /// </summary>
        public static Exception NotConnectedException { get; } = new("FTP connection is not established.");
    }
}