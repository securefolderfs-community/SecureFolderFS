using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class CleanupCallback : BaseDokanOperationsCallbackWithPath, ICleanupCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public CleanupCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
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
                var ciphertextPath = GetCiphertextPath(fileName);
                try
                {
                    if (info.IsDirectory)
                    {
                        _fileSystemOperations.PrepareDirectoryForDeletion(ciphertextPath);
                        Directory.Delete(ciphertextPath, true);
                    }
                    else
                    {
                        File.Delete(ciphertextPath);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}
