using DokanNet;
using SecureFolderFS.Sdk.Paths;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class CleanupCallback : BaseDokanOperationsCallbackWithPath, ICleanupCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public CleanupCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._fileSystemOperations = fileSystemOperations;
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
            handles.Close(GetContextValue(info));
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
                    if (_fileSystemOperations.PrepareFileForDeletion(ciphertextPath))
                    {
                        _fileSystemOperations.DangerousFileOperations.DeleteFile(ciphertextPath.Path);
                    }
                }
            }
        }
    }
}
