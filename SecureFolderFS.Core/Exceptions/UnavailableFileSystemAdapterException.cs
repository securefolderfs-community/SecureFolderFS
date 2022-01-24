using System;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Core.Exceptions
{
    public sealed class UnavailableFileSystemAdapterException : Exception
    {
        public FileSystemAvailabilityErrorType FileSystemAvailabilityErrorType { get; }

        internal UnavailableFileSystemAdapterException()
            : base("No available file system adapters exist.")
        {
        }

        internal UnavailableFileSystemAdapterException(FileSystemAdapterType fileSystemAdapterType, FileSystemAvailabilityErrorType fileSystemAvailabilityErrorType)
            : base($"The file system adapter: {fileSystemAdapterType} is unavailable.")
        {
            this.FileSystemAvailabilityErrorType = fileSystemAvailabilityErrorType;
        }
    }
}
