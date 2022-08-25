using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class DeleteDirectoryCallback : BaseDokanOperationsCallbackWithPath, IDeleteDirectoryCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public DeleteDirectoryCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
            _fileSystemOperations = fileSystemOperations;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);
            return _fileSystemOperations.CanDeleteDirectory(ciphertextPath) ? DokanResult.Success : DokanResult.DirectoryNotEmpty;
        }
    }
}
