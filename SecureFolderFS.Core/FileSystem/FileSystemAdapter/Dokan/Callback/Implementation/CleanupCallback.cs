using DokanNet;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class CleanupCallback : BaseDokanOperationsCallbackWithPath, ICleanupCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public CleanupCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
            _fileSystemOperations = fileSystemOperations;
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
            handles.CloseHandle(GetContextValue(info));
            InvalidateContext(info);

            // Make sure we delete redirected items from DeleteDirectory() and DeleteFile() here.
            if (info.DeleteOnClose)
            {
                ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

                if (info.IsDirectory)
                {
                    _fileSystemOperations.PrepareDirectoryForDeletion(ciphertextPath);
                    _fileSystemOperations.DangerousDirectoryOperations.DeleteDirectory(ciphertextPath.Path, true);
                }
                else
                {
                    _fileSystemOperations.DangerousFileOperations.DeleteFile(ciphertextPath.Path);
                }
            }
        }
    }
}
