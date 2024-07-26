using DokanNet;
using SecureFolderFS.Core.Dokany.AppModels;
using SecureFolderFS.Core.Dokany.Helpers;
using SecureFolderFS.Core.Dokany.OpenHandles;
using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Helpers.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.Native;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using FileAccess = DokanNet.FileAccess;

namespace SecureFolderFS.Core.Dokany.Callbacks
{
    internal sealed class OnDeviceDokany : BaseDokanyCallbacks, IDokanOperationsUnsafe
    {
        private DriveInfo? _vaultDriveInfo;
        private int _vaultDriveInfoTries;

        public OnDeviceDokany(FileSystemSpecifics specifics, BaseHandlesManager handlesManager, DokanyVolumeModel volumeModel)
            : base(specifics, handlesManager, volumeModel)
        {
        }

        /// <inheritdoc/>
        public override NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            var result = DokanResult.Success;
            var ciphertextPath = GetCiphertextPath(fileName);
            if (ciphertextPath is null)
                return Trace(DokanResult.PathNotFound, fileName, info, access, share, mode, options, attributes);

            if (info.IsDirectory)
            {
                try
                {
                    switch (mode)
                    {
                        case FileMode.Open:
                        {
                            if (!Directory.Exists(ciphertextPath))
                            {
                                try
                                {
                                    if (!File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory))
                                        return Trace(DokanResult.NotADirectory, fileName, info, access, share, mode, options, attributes);
                                }
                                catch (Exception)
                                {
                                    return Trace(DokanResult.FileNotFound, fileName, info, access, share, mode, options, attributes);
                                }

                                return Trace(DokanResult.PathNotFound, fileName, info, access, share, mode, options, attributes);
                            }

                            // .Any() iterator moves by one - corresponds to FindNextFile
                            _ = new DirectoryInfo(ciphertextPath).EnumerateFileSystemInfos().Any();

                            break;
                        }

                        case FileMode.CreateNew:
                        {
                            if (Directory.Exists(ciphertextPath))
                                return Trace(DokanResult.FileExists, fileName, info, access, share, mode, options, attributes);

                            try
                            {
                                _ = File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory);
                                return Trace(DokanResult.AlreadyExists, fileName, info, access, share, mode, options, attributes);
                            }
                            catch (IOException)
                            {
                            }

                            // Create directory
                            _ = Directory.CreateDirectory(ciphertextPath);

                            // Create new DirectoryID
                            var directoryId = Guid.NewGuid().ToByteArray();
                            var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.DIRECTORY_ID_FILENAME);

                            // Initialize directory with DirectoryID
                            using var directoryIdStream = File.Open(directoryIdPath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                            directoryIdStream.Write(directoryId);

                            // Set DirectoryID to known IDs
                            Specifics.DirectoryIdCache.CacheSet(directoryIdPath, new(directoryId));
                            break;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return Trace(DokanResult.AccessDenied, fileName, info, access, share, mode, options, attributes);
                }
            }
            else
            {
                var pathExists = true;
                var pathIsDirectory = false;

                var readWriteAttributes = (access & Constants.FileSystem.DATA_ACCESS) == 0;
                var readAccess = (access & Constants.FileSystem.DATA_WRITE_ACCESS) == 0;

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
                                    return Trace(DokanResult.AccessDenied, fileName, info, access, share, mode, options, attributes);
                                }

                                info.IsDirectory = pathIsDirectory;
                                InvalidateContext(info); // Must invalidate before returning DokanResult.Success

                                return Trace(DokanResult.Success, fileName, info, access, share, mode, options, attributes);
                            }
                        }
                        else
                        {
                            return Trace(DokanResult.FileNotFound, fileName, info, access, share, mode, options, attributes);
                        }
                        break;

                    case FileMode.CreateNew:
                        if (pathExists)
                            return Trace(DokanResult.FileExists, fileName, info, access, share, mode, options, attributes);
                        break;

                    case FileMode.Truncate:
                        if (!pathExists)
                            return Trace(DokanResult.FileNotFound, fileName, info, access, share, mode, options, attributes);
                        break;
                }

                try
                {
                    var openAccess = readAccess ? System.IO.FileAccess.Read : System.IO.FileAccess.ReadWrite;
                    if (mode == FileMode.CreateNew && readAccess)
                        openAccess = System.IO.FileAccess.ReadWrite;

                    info.Context = handlesManager.OpenFileHandle(ciphertextPath, mode, openAccess, share, options);

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
                    return Trace(NtStatus.CrcError, fileName, info, access, share, mode, options, attributes);
                }
                catch (UnauthorizedAccessException) // Don't have access rights
                {
                    // Must invalidate here, because cleanup is not called
                    CloseHandle(info);
                    InvalidateContext(info);
                    return Trace(DokanResult.AccessDenied, fileName, info, access, share, mode, options, attributes);
                }
                catch (DirectoryNotFoundException)
                {
                    return Trace(DokanResult.PathNotFound, fileName, info, access, share, mode, options, attributes);
                }
                catch (IOException ioEx)
                {
                    // Already exists
                    if (ErrorHandlingHelpers.IsFileAlreadyExistsException(ioEx))
                        return Trace(DokanResult.AlreadyExists, fileName, info, access, share, mode, options, attributes);

                    // Sharing violation
                    if (ErrorHandlingHelpers.IsSharingViolationException(ioEx))
                        return Trace(DokanResult.SharingViolation, fileName, info, access, share, mode, options, attributes);

                    throw;
                }
            }

            return Trace(result, fileName, info, access, share, mode, options, attributes);
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
                        var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.DIRECTORY_ID_FILENAME);
                        Specifics.DirectoryIdCache.CacheRemove(directoryIdPath);
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
                    return Trace(DokanResult.PathNotFound, fileName, info);
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
                        ? Specifics.Security.ContentCrypt.CalculateCleartextSize(fileInfo2.Length - Specifics.Security.HeaderCrypt.HeaderCiphertextSize)
                        : 0L
                };

                return Trace(DokanResult.Success, fileName, info);
            }
            catch (PathTooLongException)
            {
                fileInfo = default;
                return Trace(DokanResult.InvalidName, fileName, info);
            }
            catch (FileNotFoundException)
            {
                fileInfo = default;
                return Trace(DokanResult.FileNotFound, fileName, info);
            }
            catch (DirectoryNotFoundException)
            {
                fileInfo = default;
                return Trace(DokanResult.PathNotFound, fileName, info);
            }
            catch (UnauthorizedAccessException)
            {
                fileInfo = default;
                return Trace(DokanResult.AccessDenied, fileName, info);
            }
            catch (Exception)
            {
                fileInfo = default;
                return Trace(DokanResult.Unsuccessful, fileName, info);
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
                    di.RootDirectory.Name.Equals(Path.GetPathRoot(Specifics.ContentFolder.Id), StringComparison.OrdinalIgnoreCase));
            }

            freeBytesAvailable = _vaultDriveInfo?.TotalFreeSpace ?? 0L;
            totalNumberOfBytes = _vaultDriveInfo?.TotalSize ?? 0L;
            totalNumberOfFreeBytes = _vaultDriveInfo?.AvailableFreeSpace ?? 0L;

            return Trace(DokanResult.Success, null, info);
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
                    return Trace(DokanResult.PathNotFound, fileName, info);
                }

                var directory = new DirectoryInfo(ciphertextPath);
                List<FileInformation>? fileList = null;

                var directoryId = AbstractPathHelpers.AllocateDirectoryId(Specifics, fileName);
                var itemsEnumerable = Specifics.Security.NameCrypt is null ? directory.EnumerateFileSystemInfos(searchPattern) : directory.EnumerateFileSystemInfos();

                foreach (var item in itemsEnumerable)
                {
                    if (PathHelpers.IsCoreFile(item.Name))
                        continue;

                    var plaintextName = NativePathHelpers.GetPlaintextPath(item.FullName, Specifics, directoryId);
                    plaintextName = plaintextName is not null
                        ? Path.GetFileName(plaintextName)
                        : item.Name;

                    if (string.IsNullOrEmpty(plaintextName))
                        continue;

                    if (Specifics.Security.NameCrypt is not null && !UnsafeNativeApis.PathMatchSpec(plaintextName, searchPattern))
                        continue;

                    fileList ??= new();
                    fileList.Add(new FileInformation()
                    {
                        FileName = plaintextName,
                        Attributes = item.Attributes,
                        CreationTime = item.CreationTime,
                        LastAccessTime = item.LastAccessTime,
                        LastWriteTime = item.LastWriteTime,
                        Length = item is FileInfo fileInfo
                            ? Specifics.Security.ContentCrypt.CalculateCleartextSize(fileInfo.Length - Specifics.Security.HeaderCrypt.HeaderCiphertextSize)
                            : 0L
                    });
                }

                files = fileList is null ? Array.Empty<FileInformation>() : fileList;
                return Trace(DokanResult.Success, fileName, info);
            }
            catch (PathTooLongException)
            {
                files = Array.Empty<FileInformation>();
                return Trace(DokanResult.InvalidName, fileName, info);
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
                        return Trace(DokanResult.PathNotFound, fileName, info);

                    File.SetAttributes(ciphertextPath, attributes);
                }

                return Trace(DokanResult.Success, fileName, info);
            }
            catch (PathTooLongException)
            {
                return Trace(DokanResult.InvalidName, fileName, info);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(DokanResult.AccessDenied, fileName, info);
            }
            catch (FileNotFoundException)
            {
                return Trace(DokanResult.FileNotFound, fileName, info);
            }
            catch (DirectoryNotFoundException)
            {
                return Trace(DokanResult.PathNotFound, fileName, info);
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
                            return Trace(DokanResult.Success, fileName, info);
                    }
                    else
                    {
                        return Trace(DokanResult.InvalidHandle, fileName, info);
                    }

                    var hrException = Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
                    if (hrException is not null)
                        throw hrException;
                }

                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                    return Trace(NtStatus.ObjectPathInvalid, fileName, info);

                if (creationTime is not null)
                    File.SetCreationTime(ciphertextPath, creationTime.Value);

                if (lastAccessTime is not null)
                    File.SetLastAccessTime(ciphertextPath, lastAccessTime.Value);

                if (lastWriteTime is not null)
                    File.SetLastWriteTime(ciphertextPath, lastWriteTime.Value);

                return Trace(DokanResult.Success, fileName, info);
            }
            catch (PathTooLongException)
            {
                return Trace(DokanResult.InvalidName, fileName, info);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(DokanResult.AccessDenied, fileName, info);
            }
            catch (FileNotFoundException)
            {
                return Trace(DokanResult.FileNotFound, fileName, info);
            }
        }

        /// <inheritdoc/>
        public override NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            var ciphertextPath = GetCiphertextPath(fileName);

            // Just check if we can delete the file - the true deletion is done in Cleanup()
            if (Directory.Exists(ciphertextPath))
                return Trace(DokanResult.AccessDenied, fileName, info);

            if (!File.Exists(ciphertextPath))
                return Trace(DokanResult.FileNotFound, fileName, info);

            if (File.GetAttributes(ciphertextPath).HasFlag(FileAttributes.Directory))
                return Trace(DokanResult.AccessDenied, fileName, info);

            return Trace(DokanResult.Success, fileName, info);
        }

        /// <inheritdoc/>
        public override NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            var canDelete = true;
            var ciphertextPath = GetCiphertextPath(fileName);
            if (ciphertextPath is null)
                return Trace(NtStatus.ObjectPathInvalid, fileName, info);

            using var directoryEnumerator = Directory.EnumerateFileSystemEntries(ciphertextPath).GetEnumerator();

            while (directoryEnumerator.MoveNext())
            {
                // Check for any files except core files
                canDelete &= PathHelpers.IsCoreFile(Path.GetFileName(directoryEnumerator.Current));

                // If the flag changed (directory is not empty), break the loop
                if (!canDelete)
                    break;
            }

            var result = canDelete ? DokanResult.Success : DokanResult.DirectoryNotEmpty;
            return Trace(result, fileName, info);
        }

        /// <inheritdoc/>
        public override NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            var oldCiphertextPath = GetCiphertextPath(oldName);
            var newCiphertextPath = GetCiphertextPath(newName);

            var fileNameCombined = $"{oldName} -> {newName}";

            if (oldCiphertextPath is null || newCiphertextPath is null)
                return Trace(NtStatus.ObjectPathInvalid, fileNameCombined, info);

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

                    return Trace(DokanResult.Success, fileNameCombined, info);
                }
                else if (replace)
                {
                    InvalidateContext(info);
                    if (info.IsDirectory)
                    {
                        // Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
                        return Trace(DokanResult.AccessDenied, fileNameCombined, info);
                    }

                    // File
                    File.Delete(newCiphertextPath);
                    File.Move(oldCiphertextPath, newCiphertextPath);
                    
                    return Trace(DokanResult.Success, fileNameCombined, info);
                }
                else
                    return Trace(DokanResult.FileExists, fileNameCombined, info);
            }
            catch (PathTooLongException)
            {
                return Trace(DokanResult.InvalidName, fileNameCombined, info);
            }
            catch (FileNotFoundException)
            {
                return Trace(DokanResult.FileNotFound, fileNameCombined, info);
            }
            catch (DirectoryNotFoundException)
            {
                return Trace(DokanResult.PathNotFound, fileNameCombined, info);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(DokanResult.AccessDenied, fileNameCombined, info);
            }
            catch (IOException ioEx)
            {
                if (ErrorHandlingHelpers.IsFileAlreadyExistsException(ioEx))
                    return Trace(DokanResult.AlreadyExists, fileNameCombined, info);

                if (ErrorHandlingHelpers.IsDirectoryNotEmptyException(ioEx))
                    return Trace(DokanResult.DirectoryNotEmpty, fileNameCombined, info);

                if (ErrorHandlingHelpers.IsDiskFullException(ioEx))
                    return Trace(DokanResult.DiskFull, fileNameCombined, info);

                if (ErrorHandlingHelpers.NtStatusFromException(ioEx, out var ntStatus))
                    return Trace((NtStatus)ntStatus, fileNameCombined, info);

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
                    return Trace((NtStatus)ntStatus, fileName, info);

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
                    return Trace(DokanResult.Success, fileName, info);
                }
                
                return Trace(DokanResult.InvalidHandle, fileName, info);
            }
            catch (IOException)
            {
                return Trace(DokanResult.AccessDenied, fileName, info);
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
                    return Trace(DokanResult.Success, fileName, info);
                }

                return Trace(DokanResult.InvalidHandle, fileName, info);
            }
            catch (IOException)
            {
                return Trace(DokanResult.AccessDenied, fileName, info);
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
                    return Trace(DokanResult.PathNotFound, fileName, info);
                }

#pragma warning disable CA1416 // Validate platform compatibility
                security = info.IsDirectory
                    ? new DirectoryInfo(ciphertextPath).GetAccessControl()
                    : new FileInfo(ciphertextPath).GetAccessControl();
#pragma warning restore CA1416 // Validate platform compatibility

                return Trace(DokanResult.Success, fileName, info);
            }
            catch (FileNotFoundException)
            {
                security = null;
                return Trace(DokanResult.FileNotFound, fileName, info);
            }
            catch (DirectoryNotFoundException)
            {
                security = null;
                return Trace(DokanResult.PathNotFound, fileName, info);
            }
            catch (PathTooLongException)
            {
                security = null;
                return Trace(DokanResult.InvalidName, fileName, info);
            }
            catch (UnauthorizedAccessException)
            {
                security = null;
                return Trace(DokanResult.AccessDenied, fileName, info);
            }
        }

        /// <inheritdoc/>
        public override NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);
                if (ciphertextPath is null)
                    return Trace(DokanResult.PathNotFound, fileName, info);

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

                return Trace(DokanResult.Success, fileName, info);
            }
            catch (FileNotFoundException)
            {
                return Trace(DokanResult.FileNotFound, fileName, info);
            }
            catch (DirectoryNotFoundException)
            {
                return Trace(DokanResult.PathNotFound, fileName, info);
            }
            catch (PathTooLongException)
            {
                return Trace(DokanResult.InvalidName, fileName, info);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(DokanResult.AccessDenied, fileName, info);
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
                    return Trace((NtStatus)ntStatus, fileName, info);
                }

                throw;
            }
            catch (Exception ex)
            {
                _ = ex;
                bytesRead = 0;
                Debugger.Break();

                return Trace(DokanResult.InternalError, fileName, info);
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
                    return Trace((NtStatus)ntStatus, fileName, info);
                }

                throw;
            }
            catch (Exception ex)
            {
                _ = ex;
                bytesWritten = 0;

                Debugger.Break();
                return Trace(DokanResult.InternalError, fileName, info);
            }
        }

        /// <inheritdoc/>
        public override NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            return base.FindStreams(fileName, out streams, info);
        }

        // TODO: Remove this method and call NativePathHelpers.GetCiphertextPath() instead
        /// <inheritdoc/>
        protected override string? GetCiphertextPath(string cleartextName)
        {
            var directoryId = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
            return NativePathHelpers.GetCiphertextPath(cleartextName, Specifics, directoryId);
        }
    }
}
