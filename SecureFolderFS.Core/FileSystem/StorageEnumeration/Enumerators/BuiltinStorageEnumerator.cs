using System.Collections.Generic;
using System.IO;
using System.Linq;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Helpers;

namespace SecureFolderFS.Core.FileSystem.StorageEnumeration.Enumerators
{
    internal sealed class BuiltinStorageEnumerator : IStorageEnumerator
    {
        private readonly IFileOperations _fileOperations;

        private readonly IDirectoryOperations _directoryOperations;

        private readonly IFileSystemHelpers _fileSystemHelpers;

        public BuiltinStorageEnumerator(IFileOperations fileOperations, IDirectoryOperations directoryOperations, IFileSystemHelpers fileSystemHelpers)
        {
            this._fileOperations = fileOperations;
            this._directoryOperations = directoryOperations;
            this._fileSystemHelpers = fileSystemHelpers;
        }

        public IEnumerable<FileEnumerationInfo> EnumerateFileSystemEntries(string path, string searchPattern)
        {
            return _directoryOperations
                .EnumerateFileSystemInfos(path)
                .Where((finfo) => _fileSystemHelpers.IsNameInExpression(searchPattern, finfo.Name, true))
                .Select((fileSystemInfo) => GetConverted(new FileEnumerationInfo()
                {
                    FileName = fileSystemInfo.Name,
                    Attributes = fileSystemInfo.Attributes,
                    CreationTime = fileSystemInfo.CreationTime,
                    LastAccessTime = fileSystemInfo.LastAccessTime,
                    LastWriteTime = fileSystemInfo.LastWriteTime
                }, fileSystemInfo));
        }

        public FileEnumerationInfo GetFileInfo(string path)
        {
            FileSystemInfo fileSystemInfo = _fileOperations.GetAttributes(path).HasFlag(FileAttributes.Directory)
                ? new DirectoryInfo(path)
                : new FileInfo(path);

            return GetConverted(new FileEnumerationInfo()
            {
                FileName = Path.GetFileName(path),
                Attributes = fileSystemInfo.Attributes,
                CreationTime = fileSystemInfo.CreationTime,
                LastAccessTime = fileSystemInfo.LastAccessTime,
                LastWriteTime = fileSystemInfo.LastWriteTime,
            }, fileSystemInfo);
        }

        private static FileEnumerationInfo GetConverted(FileEnumerationInfo fileEnumerationInfo, FileSystemInfo fileSystemInfo)
        {
            return fileSystemInfo is FileInfo fileInfo ? fileEnumerationInfo.AsFile(fileInfo.Length) : fileEnumerationInfo.AsFolder();
        }
    }
}
