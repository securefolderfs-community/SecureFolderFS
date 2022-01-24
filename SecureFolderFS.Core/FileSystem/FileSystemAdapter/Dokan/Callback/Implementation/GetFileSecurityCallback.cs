using DokanNet;
using System;
using System.IO;
using System.Security.AccessControl;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class GetFileSecurityCallback : BaseDokanOperationsCallbackWithPath, IGetFileSecurityCallback
    {
        public GetFileSecurityCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            if (!PlatformDataModel.IsPlatformWindows)
            {

                security = null;
                return DokanResult.NotImplemented;
            }

            try
            {
                ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

                security = info.IsDirectory
                   ? new DirectoryInfo(ciphertextPath.Path).GetAccessControl()
                   : new FileInfo(ciphertextPath.Path).GetAccessControl();

                return DokanResult.Success;
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
