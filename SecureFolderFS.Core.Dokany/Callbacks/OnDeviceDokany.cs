using DokanNet;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Helpers;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using SecureFolderFS.Core.Dokany.UnsafeNative;
using FileAccess = DokanNet.FileAccess;

namespace SecureFolderFS.Core.Dokany.Callbacks
{
    internal sealed class OnDeviceDokany : BaseDokanyCallbacks, IDokanOperationsUnsafe
    {
        private DriveInfo? _vaultDriveInfo;
        private int _vaultDriveInfoTries;
        
        public required ILocatableFolder LocatableContentFolder { get; init; }

        public required Security Security { get; init; }

        public required IDirectoryIdAccess DirectoryIdAccess { get; init; }

        public OnDeviceDokany(IPathConverter pathConverter, HandlesManager handlesManager, DokanyVolumeModel volumeModel, IFileSystemHealthStatistics? fileSystemHealthStatistics)
            : base(pathConverter, handlesManager, volumeModel, fileSystemHealthStatistics)
        {
        }

        /// <inheritdoc/>
        public override NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);
            var result = DokanResult.Success;

            if (info.IsDirectory)
            {
                try
                {
                    switch (mode)
                    {
                        case FileMode.Open:
                            if (!Directory.Exists(ciphertextPath))
                            {
                                try
                                {
                                    if (!File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory))
                                        return DokanResult.NotADirectory;
                                }
                                catch (Exception)
                                {
                                    return DokanResult.FileNotFound;
                                }

                                return DokanResult.PathNotFound;
                            }

                            _ = new DirectoryInfo(ciphertextPath).EnumerateFileSystemInfos().Any(); // .Any() iterator moves by one - corresponds to FindNextFile
                            break;

                        case FileMode.CreateNew:
                            if (Directory.Exists(ciphertextPath))
                                return DokanResult.FileExists;

                            try
                            {
                                _ = File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory);
                                return DokanResult.AlreadyExists;
                            }
                            catch (IOException)
                            {
                            }

                            // Create directory
                            _ = Directory.CreateDirectory(ciphertextPath);

                            // Initialize directory with directory ID
                            var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.DIRECTORY_ID_FILENAME);
                            _ = DirectoryIdAccess.SetDirectoryId(directoryIdPath, Guid.NewGuid().ToByteArray()); // TODO: Maybe nodiscard?

                            break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return DokanResult.AccessDenied;
                }
            }
            else
            {
                var pathExists = true;
                var pathIsDirectory = false;

                var readWriteAttributes = access.HasFlag(Constants.FileSystem.DATA_ACCESS);
                var readAccess = access.HasFlag(Constants.FileSystem.DATA_WRITE_ACCESS);

                try
                {
                    pathExists = (Directory.Exists(ciphertextPath) || File.Exists(ciphertextPath));
                    pathIsDirectory = pathExists && File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory);
                }
                catch (IOException)
                {
                }

                switch (mode)
                {
                    case FileMode.Open:

                        if (pathExists)
                        {
                            // Check if driver only wants to read attributes, security info, or open directory
                            if (readWriteAttributes || pathIsDirectory)
                            {
                                if (pathIsDirectory && (access & FileAccess.Delete) == FileAccess.Delete && (access & FileAccess.Synchronize) != FileAccess.Synchronize)
                                {
                                    // It is a DeleteFile request on a directory
                                    return DokanResult.AccessDenied;
                                }

                                info.IsDirectory = pathIsDirectory;
                                InvalidateContext(info); // Must invalidate before returning DokanResult.Success

                                return DokanResult.Success;
                            }
                        }
                        else
                        {
                            return DokanResult.FileNotFound;
                        }
                        break;

                    case FileMode.CreateNew:
                        if (pathExists)
                            return DokanResult.FileExists;
                        break;

                    case FileMode.Truncate:
                        if (!pathExists)
                            return DokanResult.FileNotFound;
                        break;
                }

                try
                {
                    var openAccess = readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite;
                    info.Context = handlesManager.OpenHandleToFile(ciphertextPath, mode, openAccess, share, options);

                    if (pathExists && (mode == FileMode.OpenOrCreate || mode == FileMode.Create))
                        result = DokanResult.AlreadyExists;

                    var fileCreated = mode == FileMode.CreateNew || mode == FileMode.Create || (!pathExists && mode == FileMode.OpenOrCreate);
                    if (fileCreated)
                    {
                        var attributes2 = attributes;
                        attributes2 |= FileAttributes.Archive; // Files are always created with FileAttributes.Archive

                        // FILE_ATTRIBUTE_NORMAL is override if any other attribute is set
                        attributes2 &= ~FileAttributes.Normal;
                        File.SetAttributes(ciphertextPath, attributes2);
                    }
                }
                catch (CryptographicException)
                {
                    // Must invalidate here, because cleanup is not called
                    CloseHandle(info);
                    InvalidateContext(info);
                    return NtStatus.CrcError;
                }
                catch (UnauthorizedAccessException) // Don't have access rights
                {
                    // Must invalidate here, because cleanup is not called
                    CloseHandle(info);
                    InvalidateContext(info);
                    return DokanResult.AccessDenied;
                }
                catch (DirectoryNotFoundException)
                {
                    return DokanResult.PathNotFound;
                }
                catch (IOException ioEx)
                {
                    // Already exists
                    if (ErrorHandlingHelpers.IsFileAlreadyExistsException(ioEx))
                        return DokanResult.AlreadyExists;

                    // Sharing violation
                    if (ErrorHandlingHelpers.IsSharingViolationException(ioEx))
                        return DokanResult.SharingViolation;

                    throw;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override void Cleanup(string fileName, IDokanFileInfo info)
        {
            CloseHandle(info);
            InvalidateContext(info);

            // Make sure we delete redirected items from DeleteDirectory() and DeleteFile() here.
            if (info.DeleteOnClose)
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                    return;

                try
                {
                    if (info.IsDirectory)
                    {
                        DirectoryIdAccess.RemoveDirectoryId(ciphertextPath);
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

        /// <inheritdoc/>
        public override NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                {
                    fileInfo = default;
                    return DokanResult.PathNotFound;
                }

                FileSystemInfo finfo = new FileInfo(ciphertextPath);
                if (!finfo.Exists)
                    finfo = new DirectoryInfo(ciphertextPath);
                
                fileInfo = new FileInformation()
                {
                    FileName = finfo.Name,
                    Attributes = finfo.Attributes,
                    CreationTime = finfo.CreationTime,
                    LastAccessTime = finfo.LastAccessTime,
                    LastWriteTime = finfo.LastWriteTime,
                    Length = finfo is FileInfo fileInfo2
                        ? Security.ContentCrypt.CalculateCleartextSize(fileInfo2.Length - Security.HeaderCrypt.HeaderCiphertextSize)
                        : 0L
                };

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                fileInfo = default;
                return DokanResult.InvalidName;
            }
            catch (FileNotFoundException)
            {
                fileInfo = default;
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                fileInfo = default;
                return DokanResult.PathNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                fileInfo = default;
                return DokanResult.AccessDenied;
            }
            catch (Exception)
            {
                fileInfo = default;
                return DokanResult.Unsuccessful;
            }
        }

        /// <inheritdoc/>
        public override NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            if (_vaultDriveInfo is null && _vaultDriveInfoTries < Constants.FileSystem.MAX_DRIVE_INFO_CALLS_UNTIL_GIVE_UP)
            {
                _vaultDriveInfoTries++;
                _vaultDriveInfo ??= DriveInfo.GetDrives().SingleOrDefault(di => 
                    di.IsReady &&
                    di.RootDirectory.Name.Equals(Path.GetPathRoot(LocatableContentFolder.Path), StringComparison.OrdinalIgnoreCase));
            }

            freeBytesAvailable = _vaultDriveInfo?.TotalFreeSpace ?? 0L;
            totalNumberOfBytes = _vaultDriveInfo?.TotalSize ?? 0L;
            totalNumberOfFreeBytes = _vaultDriveInfo?.AvailableFreeSpace ?? 0L;

            return DokanResult.Success;
        }

        /// <inheritdoc/>
        public override NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                {
                    files = Array.Empty<FileInformation>();
                    return DokanResult.PathNotFound;
                }

                var directory = new DirectoryInfo(ciphertextPath);
                List<FileInformation>? fileList = null;

                var items = Security.NameCrypt is null ? directory.EnumerateFileSystemInfos(searchPattern) : directory.EnumerateFileSystemInfos();
                foreach (var item in items)
                {
                    if (PathHelpers.IsCoreFile(item.Name))
                        continue;

                    var cleartextName = pathConverter.GetCleartextFileName(item.FullName);
                    if (string.IsNullOrEmpty(cleartextName))
                        continue;

                    if (Security.NameCrypt is not null && !UnsafeNativeApis.PathMatchSpec(cleartextName, searchPattern))
                        continue;

                    fileList ??= new();
                    fileList.Add(new FileInformation()
                    {
                        FileName = cleartextName,
                        Attributes = item.Attributes,
                        CreationTime = item.CreationTime,
                        LastAccessTime = item.LastAccessTime,
                        LastWriteTime = item.LastWriteTime,
                        Length = item is FileInfo fileInfo
                            ? Security.ContentCrypt.CalculateCleartextSize(fileInfo.Length - Security.HeaderCrypt.HeaderCiphertextSize)
                            : 0L
                    });
                }

                files = fileList is null ? Array.Empty<FileInformation>() : fileList;
                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                files = Array.Empty<FileInformation>();
                return DokanResult.InvalidName;
            }
        }

        /// <inheritdoc/>
        public override NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            try
            {
                // MS-FSCC 2.6 File Attributes : There is no file attribute with the value 0x00000000
                // because a value of 0x00000000 in the FileAttributes field means that the file attributes for this file MUST NOT be changed when setting basic information for the file
                if (attributes != 0)
                {
                    var ciphertextPath = GetCiphertextPath(fileName);
                    if (ciphertextPath is null)
                        return DokanResult.PathNotFound;

                    File.SetAttributes(ciphertextPath, attributes);
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
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return DokanResult.PathNotFound;
            }
        }

        /// <inheritdoc/>
        public override NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            try
            {
                if (!IsContextInvalid(info))
                {
                    var ct = creationTime?.ToFileTime() ?? 0L;
                    var lat = lastAccessTime?.ToFileTime() ?? 0L;
                    var lwt = lastWriteTime?.ToFileTime() ?? 0L;

                    if (handlesManager.GetHandle<Win32FileHandle>(GetContextValue(info)) is { } fileHandle)
                    {
                        if (fileHandle.SetFileTime(ref ct, ref lat, ref lwt))
                            return DokanResult.Success;
                    }
                    else
                    {
                        return DokanResult.InvalidHandle;
                    }

                    var hrException = Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
                    if (hrException is not null)
                        throw hrException;
                }

                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                    return NtStatus.ObjectPathInvalid;

                if (creationTime is not null)
                    File.SetCreationTime(ciphertextPath, creationTime.Value);

                if (lastAccessTime is not null)
                    File.SetLastAccessTime(ciphertextPath, lastAccessTime.Value);

                if (lastWriteTime is not null)
                    File.SetLastWriteTime(ciphertextPath, lastWriteTime.Value);

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
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
        }

        /// <inheritdoc/>
        public override NtStatus DeleteFile(string fileName, IDokanFileInfo info)
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

        /// <inheritdoc/>
        public override NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            var canDelete = true;
            var ciphertextPath = GetCiphertextPath(fileName);
            if (ciphertextPath is null)
                return NtStatus.ObjectPathInvalid;

            using var directoryEnumerator = Directory.EnumerateFileSystemEntries(ciphertextPath).GetEnumerator();

            while (directoryEnumerator.MoveNext())
            {
                // Check for any files except core files
                canDelete &= PathHelpers.IsCoreFile(Path.GetFileName(directoryEnumerator.Current));

                // If the flag changed (directory is not empty), break the loop
                if (!canDelete)
                    break;
            }

            return canDelete ? DokanResult.Success : DokanResult.DirectoryNotEmpty;
        }

        /// <inheritdoc/>
        public override NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            var oldCiphertextPath = GetCiphertextPath(oldName);
            var newCiphertextPath = GetCiphertextPath(newName);

            if (oldCiphertextPath is null || newCiphertextPath is null)
                return NtStatus.ObjectPathInvalid;

            CloseHandle(info);
            InvalidateContext(info);

            var newPathExists = info.IsDirectory ? Directory.Exists(newCiphertextPath) : File.Exists(newCiphertextPath);

            try
            {
                if (!newPathExists)
                {
                    InvalidateContext(info);
                    if (info.IsDirectory)
                    {
                        Directory.Move(oldCiphertextPath, newCiphertextPath);
                    }
                    else
                    {
                        File.Move(oldCiphertextPath, newCiphertextPath);
                    }

                    return DokanResult.Success;
                }
                else if (replace)
                {
                    InvalidateContext(info);
                    if (info.IsDirectory)
                    {
                        // Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
                        return DokanResult.AccessDenied;
                    }

                    // File
                    File.Delete(newCiphertextPath);
                    File.Move(oldCiphertextPath, newCiphertextPath);
                    
                    return DokanResult.Success;
                }
                else
                    return DokanResult.FileExists;
            }
            catch (PathTooLongException)
            {
                return DokanResult.InvalidName;
            }
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return DokanResult.PathNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
            catch (IOException ioEx)
            {
                if (ErrorHandlingHelpers.IsFileAlreadyExistsException(ioEx))
                    return DokanResult.AlreadyExists;

                if (ErrorHandlingHelpers.IsDirectoryNotEmptyException(ioEx))
                    return DokanResult.DirectoryNotEmpty;

                if (ErrorHandlingHelpers.IsDiskFullException(ioEx))
                    return DokanResult.DiskFull;

                if (ErrorHandlingHelpers.NtStatusFromException(ioEx, out var ntStatus))
                    return (NtStatus)ntStatus;

                throw;
            }
        }

        /// <inheritdoc/>
        public override NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            try
            {
                return base.SetEndOfFile(fileName, length, info);
            }
            catch (IOException ioEx)
            {
                if (ErrorHandlingHelpers.NtStatusFromException(ioEx, out var ntStatus))
                    return (NtStatus)ntStatus;

                throw;
            }
        }

        /// <inheritdoc/>
        public override NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            try
            {
                if (handlesManager.GetHandle<Win32FileHandle>(GetContextValue(info)) is { } fileHandle)
                {
                    fileHandle.Lock(offset, length);
                    return DokanResult.Success;
                }
                
                return DokanResult.InvalidHandle;
            }
            catch (IOException)
            {
                return DokanResult.AccessDenied;
            }
        }

        /// <inheritdoc/>
        public override NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            try
            {
                if (handlesManager.GetHandle<Win32FileHandle>(GetContextValue(info)) is { } fileHandle)
                {
                    fileHandle.Unlock(offset, length);
                    return DokanResult.Success;
                }

                return DokanResult.InvalidHandle;
            }
            catch (IOException)
            {
                return DokanResult.AccessDenied;
            }
        }

        /// <inheritdoc/>
        public override NtStatus GetFileSecurity(string fileName, out FileSystemSecurity? security, AccessControlSections sections, IDokanFileInfo info)
        {
            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                {
                    security = null;
                    return DokanResult.PathNotFound;
                }

#pragma warning disable CA1416 // Validate platform compatibility
                security = info.IsDirectory
                    ? new DirectoryInfo(ciphertextPath).GetAccessControl()
                    : new FileInfo(ciphertextPath).GetAccessControl();
#pragma warning restore CA1416 // Validate platform compatibility

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

        /// <inheritdoc/>
        public override NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                    return DokanResult.PathNotFound;

#pragma warning disable CA1416 // Validate platform compatibility
                if (info.IsDirectory)
                {
                    new DirectoryInfo(ciphertextPath).SetAccessControl((DirectorySecurity)security);
                }
                else
                {
                    new FileInfo(ciphertextPath).SetAccessControl((FileSecurity)security);
                }
#pragma warning restore CA1416 // Validate platform compatibility

                return DokanResult.Success;
            }
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return DokanResult.PathNotFound;
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

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info)
        {
            try
            {
                return base.ReadFile(fileName, buffer, bufferLength, out bytesRead, offset, info);
            }
            catch (IOException ioEx)
            {
                if (ErrorHandlingHelpers.NtStatusFromException(ioEx, out var ntStatus))
                {
                    bytesRead = 0;
                    return (NtStatus)ntStatus;
                }

                throw;
            }
            catch (Exception ex)
            {
                _ = ex;
                bytesRead = 0;
                Debugger.Break();

                return DokanResult.InternalError;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override NtStatus WriteFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesWritten, long offset, IDokanFileInfo info)
        {
            try
            {
                return base.WriteFile(fileName, buffer, bufferLength, out bytesWritten, offset, info);
            }
            catch (IOException ioEx)
            {
                if (ErrorHandlingHelpers.NtStatusFromException(ioEx, out var ntStatus))
                {
                    bytesWritten = 0;
                    return (NtStatus)ntStatus;
                }

                throw;
            }
            catch (Exception ex)
            {
                _ = ex;
                bytesWritten = 0;

                Debugger.Break();
                return DokanResult.InternalError;
            }
        }

        /// <inheritdoc/>
        protected override string? GetCiphertextPath(string cleartextName)
        {
            var path = PathHelpers.PathFromVaultRoot(cleartextName, LocatableContentFolder.Path);
            return pathConverter.ToCiphertext(path);
        }
    }
}
