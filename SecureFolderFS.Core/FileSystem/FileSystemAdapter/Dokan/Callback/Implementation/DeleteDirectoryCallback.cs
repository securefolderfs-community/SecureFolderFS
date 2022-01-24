using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class DeleteDirectoryCallback : BaseDokanOperationsCallbackWithPath, IDeleteDirectoryCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public DeleteDirectoryCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._fileSystemOperations = fileSystemOperations;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            if (!info.IsDirectory)
            {
                return DokanResult.NotADirectory;
            }

            ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);
            return _fileSystemOperations.CanDeleteDirectory(ciphertextPath) ? DokanResult.Success : DokanResult.DirectoryNotEmpty;
        }
    }
}
