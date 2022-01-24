using DokanNet;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class DeleteFileCallback : BaseDokanOperationsCallbackWithPath, IDeleteFileCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public DeleteFileCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._fileSystemOperations = fileSystemOperations;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

            // Just check if we can delete the file - the true deletion is done in Cleanup()
            if (_fileSystemOperations.DangerousDirectoryOperations.Exists(ciphertextPath.Path))
            {
                return DokanResult.AccessDenied;
            }
            else if (!_fileSystemOperations.DangerousFileOperations.Exists(ciphertextPath.Path))
            {
                return DokanResult.FileNotFound;
            }
            else if (_fileSystemOperations.DangerousFileOperations.GetAttributes(ciphertextPath.Path).HasFlag(FileAttributes.Directory))
            {
                return DokanResult.AccessDenied;
            }
            else if (handles.GetHandle(GetContextValue(info)) is FileHandle fileHandle)
            {
                return fileHandle.CleartextFileStream.CanBeDeleted() ? DokanResult.Success : DokanResult.AccessDenied;
            }
            else
            {
                return DokanResult.InvalidHandle;
            }
        }
    }
}
