using DokanNet;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using System;
using System.IO;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class CleanupCallback : BaseDokanCallback, ICleanupCallback
    {
        private readonly IDirectoryIdAccess _directoryIdAccess;

        public CleanupCallback(IDirectoryIdAccess directoryIdAccess, string vaultRootPath, IPathConverter pathConverter, HandlesManager handlesManager)
            : base(vaultRootPath, pathConverter, handlesManager)
        {
            _directoryIdAccess = directoryIdAccess;
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
            handlesManager.CloseHandle(GetContextValue(info));
            InvalidateContext(info);

            // Make sure we delete redirected items from DeleteDirectory() and DeleteFile() here.
            if (info.DeleteOnClose)
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                try
                {
                    if (info.IsDirectory)
                    {
                        _directoryIdAccess.RemoveDirectoryId(ciphertextPath);
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
