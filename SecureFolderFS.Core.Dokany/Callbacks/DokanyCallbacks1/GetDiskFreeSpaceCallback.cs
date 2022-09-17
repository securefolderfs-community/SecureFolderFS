using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using System;
using System.IO;
using System.Linq;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class GetDiskFreeSpaceCallback : BaseDokanOperationsCallbackWithPath, IGetDiskFreeSpaceCallback
    {
        private DriveInfo _secureFolderDriveInfo;

        private int _driveInfoTries;

        public GetDiskFreeSpaceCallback(VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
        {
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            if (_driveInfoTries < Constants.FileSystem.MAX_DRIVE_INFO_CALLS_UNTIL_GIVEUP && _secureFolderDriveInfo is null)
            {
                _driveInfoTries++;
                _secureFolderDriveInfo ??= DriveInfo.GetDrives().SingleOrDefault(di => di.IsReady && string.Equals(di.RootDirectory.Name, Path.GetPathRoot(vaultPath), StringComparison.OrdinalIgnoreCase));
            }

            freeBytesAvailable = _secureFolderDriveInfo?.TotalFreeSpace ?? 0L;
            totalNumberOfBytes = _secureFolderDriveInfo?.TotalSize ?? 0L;
            totalNumberOfFreeBytes = _secureFolderDriveInfo?.AvailableFreeSpace ?? 0L;

            return DokanResult.Success;
        }
    }
}
