using DokanNet;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class DeleteFileCallback : BaseDokanOperationsCallbackWithPath, IDeleteFileCallback
    {
        public DeleteFileCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);

            // Just check if we can delete the file - the true deletion is done in Cleanup()
            if (Directory.Exists(ciphertextPath))
                return DokanResult.AccessDenied;

            if (!File.Exists(ciphertextPath))
                return DokanResult.FileNotFound;

            if (File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory))
                return DokanResult.AccessDenied;

            return DokanResult.Success;
        }
    }
}
