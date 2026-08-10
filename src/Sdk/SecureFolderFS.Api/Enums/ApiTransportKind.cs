namespace SecureFolderFS.Api.Enums
{
    /// <summary>
    /// Identifies the local IPC mechanism carrying a connection.
    /// </summary>
    public enum ApiTransportKind
    {
        /// <summary>
        /// A Windows named pipe, restricted by an explicit DACL.
        /// </summary>
        NamedPipe,

        /// <summary>
        /// A Unix domain socket, restricted by filesystem permissions.
        /// </summary>
        UnixSocket
    }
}
