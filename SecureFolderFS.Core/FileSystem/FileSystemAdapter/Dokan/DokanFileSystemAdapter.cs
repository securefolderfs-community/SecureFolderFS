using DokanNet;
using DokanNet.Logging;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan
{
    internal sealed class DokanFileSystemAdapter : IDokanOperationsUnsafe, IFileSystemAdapter
    {
        private bool _disposed;

        private DokanNet.Dokan _dokan;
        private DokanNet.DokanInstance _dokanInstance;

        public ICreateFileCallback CreateFileCallback { get; init; }

        public ICleanupCallback CleanupCallback { get; init; }

        public ICloseFileCallback CloseFileCallback { get; init; }

        public IReadFileCallback ReadFileCallback { get; init; }

        public IWriteFileCallback WriteFileCallback { get; init; }

        public IFlushFileBuffersCallback FlushFileBuffersCallback { get; init; }

        public IGetFileInformationCallback GetFileInformationCallback { get; init; }

        public IFindFilesCallback FindFilesCallback { get; init; }

        public IFindFilesWithPatternCallback FindFilesWithPatternCallback { get; init; }

        public ISetFileAttributesCallback SetFileAttributesCallback { get; init; }

        public ISetFileTimeCallback SetFileTimeCallback { get; init; }

        public IDeleteFileCallback DeleteFileCallback { get; init; }

        public IDeleteDirectoryCallback DeleteDirectoryCallback { get; init; }

        public IMoveFileCallback MoveFileCallback { get; init; }

        public ISetEndOfFileCallback SetEndOfFileCallback { get; init; }

        public ISetAllocationSizeCallback SetAllocationSizeCallback { get; init; }

        public ILockFileCallback LockFileCallback { get; init; }

        public IUnlockFileCallback UnlockFileCallback { get; init; }

        public IGetDiskFreeSpaceCallback GetDiskFreeSpaceCallback { get; init; }

        public IGetVolumeInformationCallback GetVolumeInformationCallback { get; init; }

        public IGetFileSecurityCallback GetFileSecurityCallback { get; init; }

        public ISetFileSecurityCallback SetFileSecurityCallback { get; init; }

        public IMountedCallback MountedCallback { get; init; }

        public IUnmountedCallback UnmountedCallback { get; init; }

        public IFindStreamsCallback FindStreamsCallback { get; init; }

        public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return CreateFileCallback.CreateFile(fileName, access, share, mode, options, attributes, info);
        }

        public void Cleanup(string fileName, IDokanFileInfo info)
        {
            AssertNotDisposed();

            CleanupCallback.Cleanup(fileName, info);
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
            AssertNotDisposed();

            CloseFileCallback.CloseFile(fileName, info);
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            bytesRead = 0;
            return DokanResult.NotImplemented;
        }

        // TODO: OPTIMIZE - add synchronized methodimpl
        public NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return ReadFileCallback.ReadFile(fileName, buffer, bufferLength, out bytesRead, offset, info);
        }

        // TODO: OPTIMIZE - add synchronized methodimpl
        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            bytesWritten = 0;
            return DokanResult.NotImplemented;
        }

        public NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return WriteFileCallback.WriteFile(fileName, buffer, bufferLength, out bytesWritten, offset, info);
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return FlushFileBuffersCallback.FlushFileBuffers(fileName, info);
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return GetFileInformationCallback.GetFileInformation(fileName, out fileInfo, info);
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return FindFilesCallback.FindFiles(fileName, out files, info);
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return FindFilesWithPatternCallback.FindFilesWithPattern(fileName, searchPattern, out files, info);
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return SetFileAttributesCallback.SetFileAttributes(fileName, attributes, info);
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return SetFileTimeCallback.SetFileTime(fileName, creationTime, lastAccessTime, lastWriteTime, info);
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return DeleteFileCallback.DeleteFile(fileName, info);
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return DeleteDirectoryCallback.DeleteDirectory(fileName, info);
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return MoveFileCallback.MoveFile(oldName, newName, replace, info);
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return SetEndOfFileCallback.SetEndOfFile(fileName, length, info);
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return SetAllocationSizeCallback.SetAllocationSize(fileName, length, info);
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return LockFileCallback.LockFile(fileName, offset, length, info);
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return UnlockFileCallback.UnlockFile(fileName, offset, length, info);
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return GetDiskFreeSpaceCallback.GetDiskFreeSpace(out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes, info);
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return GetVolumeInformationCallback.GetVolumeInformation(out volumeLabel, out features, out fileSystemName, out maximumComponentLength, info);
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return GetFileSecurityCallback.GetFileSecurity(fileName, out security, sections, info);
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return SetFileSecurityCallback.SetFileSecurity(fileName, security, sections, info);
        }

        public NtStatus Mounted(string mountPoint, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return MountedCallback.Mounted(mountPoint, info);
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            AssertNotDisposed();

            return UnmountedCallback.Unmounted(info);
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            AssertNotDisposed();

            return FindStreamsCallback.FindStreams(fileName, out streams, info);
        }

        public void StartFileSystem(string mountLocation)
        {
            AssertNotDisposed();

            _dokan = new(new NullLogger());
            var dokanBuilder = new DokanInstanceBuilder(_dokan)
                .ConfigureOptions(opt =>
                {
                    opt.Options = DokanOptions.CaseSensitive | DokanOptions.FixedDrive;
                    opt.UNCName = Constants.FileSystem.UNC_NAME;
                    opt.MountPoint = mountLocation;
                });

            _dokanInstance = dokanBuilder.Build(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _dokanInstance?.Dispose();
            _dokan?.Dispose();

            // Disposing of callbacks can be added here in the future

            // For now we are only disposing CreateFileCallback because it will also dispose OpenHandlesCollection.
            // Not the best solution, but one that will work.
            CreateFileCallback?.Dispose();
        }
    }
}
