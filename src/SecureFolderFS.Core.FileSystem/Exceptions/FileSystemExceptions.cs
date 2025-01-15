using System;

namespace SecureFolderFS.Core.FileSystem.Exceptions
{
    public static class FileSystemExceptions
    {
        public static Exception FileSystemReadOnly { get; } = new UnauthorizedAccessException("The file system is read-only.");

        public static Exception StreamReadOnly { get; } = new NotSupportedException("The Stream instance is read-only.");
        
        public static Exception StreamNotSeekable { get; } = new NotSupportedException("Seek is not supported for this stream.");
    }
}
