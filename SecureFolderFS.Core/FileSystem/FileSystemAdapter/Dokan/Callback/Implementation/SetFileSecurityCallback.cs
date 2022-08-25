using DokanNet;
using System;
using System.IO;
using System.Security.AccessControl;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Shared.Helpers;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class SetFileSecurityCallback : BaseDokanOperationsCallbackWithPath, ISetFileSecurityCallback
    {
        public SetFileSecurityCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            if (CompatibilityHelpers.IsPlatformWindows)
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
