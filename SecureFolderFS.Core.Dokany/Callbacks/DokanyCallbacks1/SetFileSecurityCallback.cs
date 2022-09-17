using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Shared.Helpers;
using System;
using System.IO;
using System.Security.AccessControl;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class SetFileSecurityCallback : BaseDokanOperationsCallbackWithPath, ISetFileSecurityCallback
    {
        public SetFileSecurityCallback(VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
        {
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            if (CompatibilityHelpers.IsPlatformWindows)
            {
                try
                {
                    var ciphertextPath = GetCiphertextPath(fileName);

                    if (info.IsDirectory)
                    {
                        new DirectoryInfo(ciphertextPath).SetAccessControl((DirectorySecurity)security);
                    }
                    else
                    {
                        new FileInfo(ciphertextPath).SetAccessControl((FileSecurity)security);
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
