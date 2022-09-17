using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class DeleteDirectoryCallback : BaseDokanOperationsCallbackWithPath, IDeleteDirectoryCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public DeleteDirectoryCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
        {
            _fileSystemOperations = fileSystemOperations;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);
            return _fileSystemOperations.CanDeleteDirectory(ciphertextPath) ? DokanResult.Success : DokanResult.DirectoryNotEmpty;
        }
    }
}
