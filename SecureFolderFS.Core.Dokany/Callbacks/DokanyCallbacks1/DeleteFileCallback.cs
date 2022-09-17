using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using System.IO;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class DeleteFileCallback : BaseDokanOperationsCallbackWithPath, IDeleteFileCallback
    {
        public DeleteFileCallback(VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
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
