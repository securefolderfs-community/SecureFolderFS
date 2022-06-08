using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.StorageEnumeration
{
    public sealed class FileEnumerationInfo
    {
        internal bool IsFile { get; private set; }

        public string FileName { get; init; }

        public FileAttributes Attributes { get; init; }

        public DateTime? CreationTime { get; init; }

        public DateTime? LastAccessTime { get; init; }

        public DateTime? LastWriteTime { get; init; }

        public long Length { get; private set; }

        public FileEnumerationInfo AsFolder()
        {
            IsFile = false;
            Length = 0L;
            return this;
        }

        public FileEnumerationInfo AsFile(long ciphertextLength)
        {
            IsFile = true;
            Length = ciphertextLength;
            return this;
        }
    }
}
