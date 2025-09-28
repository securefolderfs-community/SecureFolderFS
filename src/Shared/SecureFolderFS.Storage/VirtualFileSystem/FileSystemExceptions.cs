using System;

namespace SecureFolderFS.Storage.VirtualFileSystem
{
    /// <summary>
    /// Provides a collection of predefined exceptions for file system operations.
    /// </summary>
    public static class FileSystemExceptions
    {
        /// <summary>
        /// Gets an exception indicating that the file system is read-only.
        /// </summary>
        public static Exception FileSystemReadOnly { get; } = new UnauthorizedAccessException("The file system is read-only.");

        /// <summary>
        /// Gets an exception indicating that the stream instance is read-only.
        /// </summary>
        public static Exception StreamReadOnly { get; } = new NotSupportedException("The stream instance is read-only.");

        /// <summary>
        /// Gets an exception indicating that the stream instance is not readable.
        /// </summary>
        public static Exception StreamNotReadable { get; } = new NotSupportedException("The stream instance is not readable.");

        /// <summary>
        /// Gets an exception indicating that seeking is not supported for the stream.
        /// </summary>
        public static Exception StreamNotSeekable { get; } = new NotSupportedException("Seeking is not supported for this stream.");
    }
}
