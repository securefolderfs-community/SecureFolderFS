using DokanNet;
using System;
using System.IO;
using System.Security.AccessControl;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class SetFileSecurityCallback : BaseDokanOperationsCallbackWithPath, ISetFileSecurityCallback
    {
        public SetFileSecurityCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            if (PlatformDataModel.IsPlatformWindows)
            {
                try
                {
                    ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

                    if (info.IsDirectory)
                    {
                        new DirectoryInfo(ciphertextPath.Path).SetAccessControl((DirectorySecurity)security);
                    }
                    else
                    {
                        new FileInfo(ciphertextPath.Path).SetAccessControl((FileSecurity)security);
                    }

                    return DokanResult.Success;
                }
                catch (PathTooLongException)
                {
                    return DokanResult.InvalidName;
                }
                catch (UnauthorizedAccessException)
                {
                    return DokanResult.AccessDenied;
                }
            }
            else
            {
                return DokanResult.NotImplemented;
            }
        }
    }
}
