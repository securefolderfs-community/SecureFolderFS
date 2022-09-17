namespace SecureFolderFS.Core.FileSystem.Enums
{
    // TODO: Needs docs
    public enum FileSystemCloseMethod : byte
    {
        /// <summary>
        /// Closes the file system if possible, otherwise fails.
        /// </summary>
        CloseIfPossible = 0,

        /// <summary>
        /// Tries to close the file system waiting for all processes to finish execution.
        /// </summary>
        GracefullyClose = 2,

        /// <summary>
        /// Forcefully closes the file system regardless of the state.
        /// </summary>
        ForcefullyClose = 4
    }
}
