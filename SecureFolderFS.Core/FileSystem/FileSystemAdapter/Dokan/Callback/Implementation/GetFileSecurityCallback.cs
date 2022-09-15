using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Shared.Helpers;
using System;
using System.IO;
using System.Security.AccessControl;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class GetFileSecurityCallback : BaseDokanOperationsCallbackWithPath, IGetFileSecurityCallback
    {
        public GetFileSecurityCallback(VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
        {
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            if (!CompatibilityHelpers.IsPlatformWindows)
            {
                security = null;
                return DokanResult.NotImplemented;
            }

            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);

                security = info.IsDirectory
                    ? new DirectoryInfo(ciphertextPath).GetAccessControl()
                    : new FileInfo(ciphertextPath).GetAccessControl();

                return DokanResult.Success;
            }
            catch (FileNotFoundException)
            {
                security = null;
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                security = null;
                return DokanResult.PathNotFound;
            }
            catch (PathTooLongException)
            {
                security = null;
                return DokanResult.InvalidName;
            }
            catch (UnauthorizedAccessException)
            {
                security = null;
                return DokanResult.AccessDenied;
            }
        }
    }
}
