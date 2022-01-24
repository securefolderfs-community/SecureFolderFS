using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback
{
    internal interface ICreateFileCallback : IDisposable
    {
        NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info);
    }

    internal interface ICleanupCallback
    {
        void Cleanup(string fileName, IDokanFileInfo info);
    }

    internal interface ICloseFileCallback
    {
        void CloseFile(string fileName, IDokanFileInfo info);
    }

    internal interface IReadFileCallback
    {
        NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info);
    }

    internal interface IWriteFileCallback
    {
        NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info);
    }

    internal interface IFlushFileBuffersCallback
    {
        NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info);
    }

    internal interface IGetFileInformationCallback
    {
        NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info);
    }

    internal interface IFindFilesCallback
    {
        NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info);
    }

    internal interface IFindFilesWithPatternCallback
    {
        NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info);
    }

    internal interface ISetFileAttributesCallback
    {
        NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info);
    }

    internal interface ISetFileTimeCallback
    {
        NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info);
    }

    internal interface IDeleteFileCallback
    {
        NtStatus DeleteFile(string fileName, IDokanFileInfo info);
    }

    internal interface IDeleteDirectoryCallback
    {
        NtStatus DeleteDirectory(string fileName, IDokanFileInfo info);
    }

    internal interface IMoveFileCallback
    {
        NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info);
    }

    internal interface ISetEndOfFileCallback
    {
        NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info);
    }

    internal interface ISetAllocationSizeCallback
    {
        NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info);
    }

    internal interface ILockFileCallback
    {
        NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info);
    }

    internal interface IUnlockFileCallback
    {
        NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info);
    }

    internal interface IGetDiskFreeSpaceCallback
    {
        NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info);
    }

    internal interface IGetVolumeInformationCallback
    {
        NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info);
    }

    internal interface IGetFileSecurityCallback
    {
        NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info);
    }

    internal interface ISetFileSecurityCallback
    {
        NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info);
    }

    internal interface IMountedCallback
    {
        NtStatus Mounted(IDokanFileInfo info);
    }

    internal interface IUnmountedCallback
    {
        NtStatus Unmounted(IDokanFileInfo info);
    }

    internal interface IFindStreamsCallback
    {
        NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info);
    }
}
