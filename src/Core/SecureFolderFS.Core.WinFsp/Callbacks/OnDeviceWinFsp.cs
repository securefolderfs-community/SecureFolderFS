using Fsp;
using Fsp.Interop;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.WinFsp.OpenHandles;
using SecureFolderFS.Core.WinFsp.UnsafeNative;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using FileInfo = Fsp.Interop.FileInfo;

namespace SecureFolderFS.Core.WinFsp.Callbacks
{
    public sealed partial class OnDeviceWinFsp : FileSystemBase, IDisposable
    {
        private readonly VolumeModel _volumeModel;
        private readonly FileSystemSpecifics _specifics;
        private readonly BaseHandlesManager _handlesManager;
        private readonly DriveInfo _driveInfo;
        private FileSystemHost? _host;

        public OnDeviceWinFsp(FileSystemSpecifics specifics, BaseHandlesManager handlesManager, VolumeModel volumeModel, DriveInfo driveInfo)
        {
            _specifics = specifics;
            _handlesManager = handlesManager;
            _volumeModel = volumeModel;
            _driveInfo = driveInfo;
        }

        /// <inheritdoc/>
        public override int Init(object Host)
        {
            _host = Host as FileSystemHost;
            if (_host is null)
                return STATUS_INVALID_PARAMETER;

            _host.SectorSize = Constants.WinFsp.SECTOR_SIZE;
            _host.SectorsPerAllocationUnit = Constants.WinFsp.SECTORS_PER_UNIT;
            _host.MaxComponentLength = Constants.WinFsp.MAX_COMPONENT_LENGTH;
            _host.FileInfoTimeout = Constants.WinFsp.FILE_INFO_TIMEOUT;
            _host.CaseSensitiveSearch = true;
            _host.CasePreservedNames = true;
            _host.UnicodeOnDisk = true;
            _host.PersistentAcls = true;
            _host.PostCleanupWhenModifiedOnly = true;
            _host.PassQueryDirectoryPattern = true;
            _host.FlushAndPurgeOnCleanup = true;
            _host.VolumeCreationTime = (ulong)DateTime.UtcNow.ToFileTimeUtc();
            _host.VolumeSerialNumber = 0u;

            return Trace(STATUS_SUCCESS);
        }

        /// <inheritdoc/>
        public override int GetVolumeInfo([UnscopedRef] out VolumeInfo VolumeInfo)
        {
            VolumeInfo = new VolumeInfo();

            VolumeInfo.SetVolumeLabel(_volumeModel.VolumeName);
            VolumeInfo.FreeSize = (ulong)_driveInfo.TotalFreeSpace;
            VolumeInfo.TotalSize = (ulong)_driveInfo.TotalSize;

            return Trace(STATUS_SUCCESS);
        }

        /// <inheritdoc/>
        public override int Open(
            string FileName,
            uint CreateOptions,
            uint GrantedAccess,
            [UnscopedRef] out object? FileNode,
            [UnscopedRef] out object? FileDesc,
            [UnscopedRef] out FileInfo FileInfo,
            [UnscopedRef] out string? NormalizedName)
        {
            var ciphertextPath = GetCiphertextPath(FileName);
            if (Directory.Exists(ciphertextPath))
            {
                FileDesc = _handlesManager.OpenDirectoryHandle(ciphertextPath);
            }
            else
            {
                var fileShare = FileShare.ReadWrite | FileShare.Delete;
                var fileAccess = ToFileAccess((FileSystemRights)GrantedAccess);
                var fileOptions = ToFileOptions(CreateOptions);

                FileDesc = _handlesManager.OpenFileHandle(ciphertextPath,
                    FileMode.Open,
                    fileAccess,
                    fileShare,
                    fileOptions);
            }

            FileNode = null;
            NormalizedName = null;

            var handle = _handlesManager.GetHandle<IDisposable>(GetContextValue(FileDesc));
            if (handle is null)
            {
                FileInfo = default;
                return Trace(STATUS_INVALID_HANDLE, FileName);
            }

            FileInfo = handle switch
            {
                WinFspFileHandle fileHandle => fileHandle.GetFileInfo(),
                WinFspDirectoryHandle dirHandle => dirHandle.GetFileInfo(),
                _ => default
            };

            return Trace(STATUS_SUCCESS, FileName);
        }

        /// <inheritdoc/>
        public override void Close(
            object FileNode,
            object FileDesc)
        {
            CloseHandle(FileDesc);
            InvalidateContext(out FileDesc);
        }

        /// <inheritdoc/>
        public override bool ReadDirectoryEntry(
            object FileNode,
            object FileDesc,
            string? Pattern,
            string? Marker,
            ref object? Context,
            [UnscopedRef] out string? FileName,
            [UnscopedRef] out FileInfo FileInfo)
        {
            if (_handlesManager.GetHandle<WinFspDirectoryHandle>(GetContextValue(FileDesc)) is not { } dirHandle)
            {
                FileName = null;
                FileInfo = default;

                return false;
            }

            // Default pattern = "*"
            Pattern = !string.IsNullOrEmpty(Pattern) ? Pattern.Replace('<', '*').Replace('>', '?').Replace('"', '.') : "*";

            var entries = new List<(string plaintextName, FileSystemInfo info)>();
            var dirInfo = dirHandle.DirectoryInfo;

            try
            {
                // Add directory entries
                var directoryId = AbstractPathHelpers.AllocateDirectoryId(_specifics.Security, dirInfo.FullName);
                foreach (var item in dirInfo.EnumerateFileSystemInfos(Pattern))
                {
                    if (PathHelpers.IsCoreName(item.Name))
                        continue;

                    var plaintextName = NativePathHelpers.GetPlaintextPath(item.FullName, _specifics, directoryId);
                    plaintextName = plaintextName is not null ? Path.GetFileName(plaintextName) : null;

                    if (string.IsNullOrEmpty(plaintextName))
                        continue;

                    if (_specifics.Security.NameCrypt is not null && !UnsafeNativeApis.PathMatchSpec(plaintextName, Pattern))
                        continue;

                    entries.Add((plaintextName, item));
                }
            }
            catch (Exception)
            {
                FileName = null;
                FileInfo = default;

                return false;
            }

            // Sort alphabetically for consistent enumeration
            entries.Sort((a, b) => string.Compare(a.info.Name, b.info.Name, StringComparison.OrdinalIgnoreCase));

            // Determine current enumeration index
            int index;
            if (Context is null)
            {
                index = 0;

                // If a marker was provided, start *after* that entry
                if (!string.IsNullOrEmpty(Marker))
                {
                    index = entries.FindIndex(e => string.Equals(e.info.Name, Marker, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                        index++;
                    else
                        index = 0;
                }
            }
            else
                index = (int)Context;

            // Produce the next entry, if any
            if (index < entries.Count)
            {
                var entry = entries[index];
                FileName = entry.plaintextName;
                FileInfo = entry.info switch
                {
                    System.IO.FileInfo fi => WinFspFileHandle.ToFileInfo(fi, _specifics.Security),
                    DirectoryInfo di => WinFspDirectoryHandle.ToFileInfo(di),
                    _ => throw new ArgumentOutOfRangeException(nameof(entry.info))
                };

                Context = index + 1;
                return true;
            }

            // No more entries
            FileName = null;
            FileInfo = default;

            return false;
        }

        /// <inheritdoc/>
        public override int GetFileInfo(
            object FileNode,
            object FileDesc,
            [UnscopedRef] out FileInfo FileInfo)
        {
            FileInfo? fInfo = _handlesManager.GetHandle<IDisposable>(GetContextValue(FileDesc)) switch
            {
                WinFspFileHandle fileHandle => fileHandle.GetFileInfo(),
                WinFspDirectoryHandle dirHandle => dirHandle.GetFileInfo(),
                _ => null
            };

            if (fInfo is null)
            {
                FileInfo = default;
                return Trace(STATUS_INVALID_HANDLE);
            }

            FileInfo = fInfo.Value;
            return Trace(STATUS_SUCCESS);
        }

        /// <inheritdoc/>
        public override int GetSecurityByName(
            string FileName,
            [UnscopedRef] out uint FileAttributes,
            ref byte[] SecurityDescriptor)
        {
            var ciphertextPath = GetCiphertextPath(FileName);
            var isDirectory = IsDirectory(ciphertextPath);

            try
            {
                // Get FileSystemInfo
                FileSystemInfo info = isDirectory
                    ? new DirectoryInfo(ciphertextPath)
                    : new System.IO.FileInfo(ciphertextPath);

                // Set Attributes and Security Descriptor
                FileAttributes = (uint)info.Attributes;
                SecurityDescriptor = info switch
                {
                    System.IO.FileInfo fileInfo => fileInfo.GetAccessControl().GetSecurityDescriptorBinaryForm(),
                    DirectoryInfo dirInfo => dirInfo.GetAccessControl().GetSecurityDescriptorBinaryForm(),
                    _ => []
                };

                return Trace(STATUS_SUCCESS, FileName);
            }
            catch (FileNotFoundException)
            {
                FileAttributes = 0;
                return Trace(STATUS_OBJECT_NAME_NOT_FOUND, FileName);
            }
            catch (DirectoryNotFoundException)
            {
                FileAttributes = 0;
                return Trace(STATUS_OBJECT_PATH_NOT_FOUND, FileName);
            }
        }

        /// <inheritdoc/>
        public override int GetSecurity(
            object FileNode,
            object FileDesc,
            ref byte[] SecurityDescriptor)
        {
            var descriptor = _handlesManager.GetHandle<IDisposable>(GetContextValue(FileDesc)) switch
            {
                WinFspFileHandle fileHandle => fileHandle.FileInfo.GetAccessControl().GetSecurityDescriptorBinaryForm(),
                WinFspDirectoryHandle dirHandle => dirHandle.DirectoryInfo.GetAccessControl().GetSecurityDescriptorBinaryForm(),
                _ => null
            };

            if (descriptor is null)
                return Trace(STATUS_INVALID_HANDLE);

            SecurityDescriptor = descriptor;
            return Trace(STATUS_SUCCESS);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override unsafe int Read(
            object FileNode,
            object FileDesc,
            IntPtr Buffer,
            ulong Offset,
            uint Length,
            [UnscopedRef] out uint BytesTransferred)
        {
            // Get handle
            if (_handlesManager.GetHandle<FileHandle>(GetContextValue(FileDesc)) is not { } fileHandle)
            {
                BytesTransferred = 0u;
                return Trace(STATUS_INVALID_HANDLE);
            }

            // Check EOF
            if (Offset >= (ulong)fileHandle.Stream.Length)
            {
                BytesTransferred = 0u;
                return Trace(STATUS_END_OF_FILE);
            }

            try
            {
                // Align position
                fileHandle.Stream.Position = (long)Offset;

                // Read file
                var bufferSpan = new Span<byte>(Buffer.ToPointer(), (int)Length);
                BytesTransferred = (uint)fileHandle.Stream.Read(bufferSpan);

                return Trace(STATUS_SUCCESS);
            }
            catch (Exception ex)
            {
                BytesTransferred = 0u;
                if (ErrorHandlingHelpers.Win32ErrorFromException(ex, out var win32error))
                    return Trace(NtStatusFromWin32(win32error));

                return Trace(STATUS_ACCESS_DENIED);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override unsafe int Write(
            object FileNode,
            object FileDesc,
            IntPtr Buffer,
            ulong Offset,
            uint Length,
            bool WriteToEndOfFile,
            bool ConstrainedIo,
            [UnscopedRef] out uint BytesTransferred,
            [UnscopedRef] out FileInfo FileInfo)
        {
            if (_specifics.Options.IsReadOnly)
            {
                FileInfo = default;
                BytesTransferred = 0u;

                return Trace(STATUS_ACCESS_DENIED);
            }

            // Get handle
            if (_handlesManager.GetHandle<WinFspFileHandle>(GetContextValue(FileDesc)) is not { } fileHandle)
            {
                FileInfo = default;
                BytesTransferred = 0u;

                return Trace(STATUS_INVALID_HANDLE);
            }

            // Check constrained I/O
            if (ConstrainedIo)
            {
                // If offset is beyond EOF, no bytes can be written in Constrained I/O mode
                if (Offset >= (ulong)fileHandle.Stream.Length)
                {
                    FileInfo = default;
                    BytesTransferred = 0u;

                    return Trace(STATUS_SUCCESS);
                }

                if ((Offset + Length) > (ulong)fileHandle.Stream.Length)
                    Length = (uint)((ulong)fileHandle.Stream.Length - Offset);
            }

            try
            {
                // Align position
                fileHandle.Stream.Position = WriteToEndOfFile ? fileHandle.Stream.Length : (long)Offset;

                // Write file
                var bufferSpan = new ReadOnlySpan<byte>(Buffer.ToPointer(), (int)Length);
                fileHandle.Stream.Write(bufferSpan);

                // Set transferred bytes and update file info
                BytesTransferred = Length;
                FileInfo = fileHandle.GetFileInfo();

                return Trace(STATUS_SUCCESS);
            }
            catch (Exception ex)
            {
                FileInfo = default;
                BytesTransferred = 0u;
                if (ErrorHandlingHelpers.Win32ErrorFromException(ex, out var win32error))
                    return Trace(NtStatusFromWin32(win32error));

                return Trace(STATUS_ACCESS_DENIED);
            }
        }

        /// <inheritdoc/>
        public override int GetDirInfoByName(
            object FileNode,
            object FileDesc,
            string FileName,
            [UnscopedRef] out string? NormalizedName,
            [UnscopedRef] out FileInfo FileInfo)
        {
            var ciphertextPath = GetCiphertextPath(FileName);
            if (!Directory.Exists(ciphertextPath))
            {
                FileInfo = default;
                NormalizedName = null;

                return Trace(STATUS_OBJECT_NAME_NOT_FOUND, FileName);
            }

            try
            {
                NormalizedName = null;
                FileInfo = WinFspDirectoryHandle.ToFileInfo(new DirectoryInfo(ciphertextPath));

                return Trace(STATUS_SUCCESS, FileName);
            }
            catch (Exception ex)
            {
                FileInfo = default;
                NormalizedName = null;

                if (ErrorHandlingHelpers.Win32ErrorFromException(ex, out var win32error))
                    return NtStatusFromWin32(win32error);

                return STATUS_ACCESS_DENIED;
            }
        }

        /// <inheritdoc/>
        public override int SetBasicInfo(
            object FileNode,
            object FileDesc,
            uint FileAttributes,
            ulong CreationTime,
            ulong LastAccessTime,
            ulong LastWriteTime,
            ulong ChangeTime,
            [UnscopedRef] out FileInfo FileInfo)
        {
            if (_specifics.Options.IsReadOnly)
            {
                FileInfo = default;
                return Trace(STATUS_ACCESS_DENIED);
            }

            FileAttributes = FileAttributes switch
            {
                Constants.UnsafeNative.INVALID_FILE_ATTRIBUTES => (uint)System.IO.FileAttributes.None,
                (uint)System.IO.FileAttributes.None => (uint)System.IO.FileAttributes.Normal,
                _ => FileAttributes
            };

            switch (_handlesManager.GetHandle<IDisposable>(GetContextValue(FileDesc)))
            {
                case WinFspFileHandle fileHandle:
                {
                    fileHandle.FileInfo.CreationTimeUtc = DateTime.FromFileTimeUtc((long)CreationTime);
                    fileHandle.FileInfo.LastWriteTimeUtc = DateTime.FromFileTimeUtc((long)LastWriteTime);
                    fileHandle.FileInfo.LastAccessTimeUtc = DateTime.FromFileTimeUtc((long)LastAccessTime);
                    fileHandle.FileInfo.Attributes = (FileAttributes)FileAttributes;
                    FileInfo = fileHandle.GetFileInfo();

                    break;
                }

                case WinFspDirectoryHandle dirHandle:
                {
                    dirHandle.DirectoryInfo.CreationTimeUtc = DateTime.FromFileTimeUtc((long)CreationTime);
                    dirHandle.DirectoryInfo.LastWriteTimeUtc = DateTime.FromFileTimeUtc((long)LastWriteTime);
                    dirHandle.DirectoryInfo.LastAccessTimeUtc = DateTime.FromFileTimeUtc((long)LastAccessTime);
                    dirHandle.DirectoryInfo.Attributes = (FileAttributes)FileAttributes;
                    FileInfo = dirHandle.GetFileInfo();

                    break;
                }

                default:
                {
                    FileInfo = default;
                    return Trace(STATUS_INVALID_HANDLE);
                }
            }

            return Trace(STATUS_SUCCESS);
        }

        /// <inheritdoc/>
        public override int SetFileSize(
            object FileNode,
            object FileDesc,
            ulong NewSize,
            bool SetAllocationSize,
            [UnscopedRef] out FileInfo FileInfo)
        {
            if (_specifics.Options.IsReadOnly)
            {
                FileInfo = default;
                return Trace(STATUS_ACCESS_DENIED);
            }

            // Get handle
            if (_handlesManager.GetHandle<WinFspFileHandle>(GetContextValue(FileDesc)) is not { } fileHandle)
            {
                FileInfo = default;
                return Trace(STATUS_INVALID_HANDLE);
            }

            try
            {
                // If the new AllocationSize is less than the current FileSize we must truncate the file
                if (!SetAllocationSize || (ulong)fileHandle.Stream.Length > NewSize)
                    fileHandle.Stream.SetLength((long)NewSize);

                FileInfo = fileHandle.GetFileInfo();
                return Trace(STATUS_SUCCESS);
            }
            catch (IOException ioEx)
            {
                FileInfo = default;
                if (ErrorHandlingHelpers.IsDiskFullException(ioEx))
                    return Trace(STATUS_DISK_FULL);

                if (ErrorHandlingHelpers.Win32ErrorFromException(ioEx, out var win32error))
                    return Trace(NtStatusFromWin32(win32error));

                return Trace(STATUS_ACCESS_DENIED);
            }
        }

        /// <inheritdoc/>
        public override int SetSecurity(
            object FileNode,
            object FileDesc,
            AccessControlSections Sections,
            byte[] SecurityDescriptor)
        {
            if (_specifics.Options.IsReadOnly)
                return Trace(STATUS_ACCESS_DENIED);

            // Get handle
            var handle = _handlesManager.GetHandle<IDisposable>(GetContextValue(FileDesc));
            if (handle is null)
                return Trace(STATUS_INVALID_HANDLE);

            try
            {
                // Apply to the underlying file or directory
                switch (handle)
                {
                    case WinFspFileHandle fileHandle:
                    {
                        var fileSecurity = new FileSecurity();
                        fileSecurity.SetSecurityDescriptorBinaryForm(SecurityDescriptor, Sections);
                        fileHandle.FileInfo.SetAccessControl(fileSecurity);

                        break;
                    }

                    case WinFspDirectoryHandle dirHandle:
                    {
                        var dirSecurity = new DirectorySecurity();
                        dirSecurity.SetSecurityDescriptorBinaryForm(SecurityDescriptor, Sections);
                        dirHandle.DirectoryInfo.SetAccessControl(dirSecurity);

                        break;
                    }

                    default: return Trace(STATUS_INVALID_HANDLE);
                }

                return Trace(STATUS_SUCCESS);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(STATUS_ACCESS_DENIED);
            }
            catch (Exception ex)
            {
                if (ErrorHandlingHelpers.Win32ErrorFromException(ex, out var win32error))
                    return Trace(NtStatusFromWin32(win32error));

                return Trace(STATUS_ACCESS_DENIED);
            }
        }

        /// <inheritdoc/>
        public override int Flush(
            object FileNode,
            object FileDesc,
            [UnscopedRef] out FileInfo FileInfo)
        {
            if (_handlesManager.GetHandle<WinFspFileHandle>(GetContextValue(FileDesc)) is not { } fileHandle)
            {
                FileInfo = default;
                return Trace(STATUS_INVALID_HANDLE);
            }

            try
            {
                fileHandle.Stream.Flush();
                FileInfo = fileHandle.GetFileInfo();

                return Trace(STATUS_SUCCESS);
            }
            catch (Exception ex)
            {
                FileInfo = default;
                if (ex is IOException ioEx && ErrorHandlingHelpers.IsDiskFullException(ioEx))
                    return Trace(STATUS_DISK_FULL);

                if (ErrorHandlingHelpers.Win32ErrorFromException(ex, out var win32error))
                    return Trace(NtStatusFromWin32(win32error));

                return Trace(STATUS_ACCESS_DENIED);
            }
        }

        /// <inheritdoc/>
        public override int Create(
            string FileName,
            uint CreateOptions,
            uint GrantedAccess,
            uint FileAttributes,
            byte[]? SecurityDescriptor,
            ulong AllocationSize,
            [UnscopedRef] out object? FileNode,
            [UnscopedRef] out object? FileDesc,
            [UnscopedRef] out FileInfo FileInfo,
            [UnscopedRef] out string? NormalizedName)
        {
            if (_specifics.Options.IsReadOnly)
            {
                FileNode = null;
                FileDesc = null;
                FileInfo = default;
                NormalizedName = null;

                return Trace(STATUS_ACCESS_DENIED, FileName);
            }

            IDisposable? handle = null;
            try
            {
                var ciphertextPath = GetCiphertextPath(FileName);
                if ((CreateOptions & FILE_DIRECTORY_FILE) == 0)
                {
                    FileSecurity? fileSecurity = null;
                    if (SecurityDescriptor is not null)
                    {
                        fileSecurity = new FileSecurity();
                        fileSecurity.SetSecurityDescriptorBinaryForm(SecurityDescriptor, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
                    }

                    var fileAccess = ToFileAccess((FileSystemRights)GrantedAccess);
                    var handleId = _handlesManager.OpenFileHandle(
                        ciphertextPath,
                        FileMode.CreateNew,
                        fileAccess,
                        FileShare.ReadWrite | FileShare.Delete,
                        FileOptions.None);

                    if (_handlesManager.GetHandle<WinFspFileHandle>(handleId) is not { } fileHandle)
                    {
                        FileNode = null;
                        FileDesc = null;
                        FileInfo = default;
                        NormalizedName = null;

                        return Trace(STATUS_INVALID_HANDLE, FileName);
                    }

                    FileDesc = handleId;
                    if (fileSecurity is not null)
                    {
                        try
                        {
                            fileHandle.FileInfo.SetAccessControl(fileSecurity);
                        }
                        catch (PrivilegeNotHeldException)
                        {
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                    }

                    fileHandle.FileInfo.Attributes =
                        (System.IO.FileAttributes)(FileAttributes | (uint)System.IO.FileAttributes.Archive);
                    handle = fileHandle;
                }
                else
                {
                    if (Directory.Exists(ciphertextPath))
                    {
                        FileNode = null;
                        FileDesc = null;
                        FileInfo = default;
                        NormalizedName = null;

                        return Trace(STATUS_OBJECT_NAME_COLLISION);
                    }

                    DirectorySecurity? dirSecurity = null;
                    if (SecurityDescriptor is not null)
                    {
                        dirSecurity = new DirectorySecurity();
                        dirSecurity.SetSecurityDescriptorBinaryForm(SecurityDescriptor, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
                    }

                    // Create directory
                    var directoryInfo = Directory.CreateDirectory(ciphertextPath);

                    // Create new DirectoryID
                    var directoryId = Guid.NewGuid().ToByteArray();
                    var directoryIdPath =
                        Path.Combine(ciphertextPath, FileSystem.Constants.Names.DIRECTORY_ID_FILENAME);

                    // Initialize directory with DirectoryID
                    using var directoryIdStream = File.Open(directoryIdPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                    directoryIdStream.Write(directoryId);

                    // Set DirectoryID to known IDs
                    _specifics.DirectoryIdCache.CacheSet(directoryIdPath, new(directoryId));

                    var handleId = _handlesManager.OpenDirectoryHandle(ciphertextPath);
                    if (_handlesManager.GetHandle<WinFspDirectoryHandle>(handleId) is not { } dirHandle)
                    {
                        FileNode = null;
                        FileDesc = null;
                        FileInfo = default;
                        NormalizedName = null;

                        return Trace(STATUS_INVALID_HANDLE, FileName);
                    }

                    FileDesc = handleId;
                    if (dirSecurity is not null)
                    {
                        try
                        {
                            directoryInfo.SetAccessControl(dirSecurity);
                        }
                        catch (PrivilegeNotHeldException)
                        {
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                    }

                    directoryInfo.Attributes = (System.IO.FileAttributes)FileAttributes;
                    handle = dirHandle;
                }

                FileNode = null;
                NormalizedName = null;

                FileInfo? fInfo = handle switch
                {
                    WinFspFileHandle fileHandle => fileHandle.GetFileInfo(),
                    WinFspDirectoryHandle dirHandle => dirHandle.GetFileInfo(),
                    _ => null
                };

                if (fInfo is null)
                {
                    FileInfo = default;
                    return Trace(STATUS_INVALID_HANDLE);
                }

                FileInfo = fInfo.Value;
                return Trace(STATUS_SUCCESS, FileName);
            }
            catch (IOException ioEx)
            {
                FileNode = null;
                FileDesc = null;
                FileInfo = default;
                NormalizedName = null;

                handle?.Dispose();
                if (ErrorHandlingHelpers.IsFileAlreadyExistsException(ioEx))
                    return Trace(STATUS_OBJECT_NAME_COLLISION, FileName);

                if (ErrorHandlingHelpers.Win32ErrorFromException(ioEx, out var win32error))
                    return Trace(NtStatusFromWin32(win32error), FileName);

                return Trace(STATUS_ACCESS_DENIED, FileName);
            }
        }

        /// <inheritdoc/>
        public override int Overwrite(
            object FileNode,
            object FileDesc,
            uint FileAttributes,
            bool ReplaceFileAttributes,
            ulong AllocationSize,
            [UnscopedRef] out FileInfo FileInfo)
        {
            if (_specifics.Options.IsReadOnly)
            {
                FileInfo = default;
                return Trace(STATUS_ACCESS_DENIED);
            }

            // Get handle
            if (_handlesManager.GetHandle<WinFspFileHandle>(GetContextValue(FileDesc)) is not { } fileHandle)
            {
                FileInfo = default;
                return Trace(STATUS_INVALID_HANDLE);
            }

            if (ReplaceFileAttributes)
            {
                fileHandle.FileInfo.Attributes = (System.IO.FileAttributes)(FileAttributes | (uint)System.IO.FileAttributes.Archive);
            }
            else if (FileAttributes != 0u)
            {
                var existingAttributes = fileHandle.FileInfo.Attributes;
                existingAttributes |= (System.IO.FileAttributes)FileAttributes;
                existingAttributes |= System.IO.FileAttributes.Archive;

                fileHandle.FileInfo.Attributes = existingAttributes;
            }

            fileHandle.Stream.SetLength(0L);
            FileInfo = fileHandle.GetFileInfo();

            return Trace(STATUS_SUCCESS);
        }

        /// <inheritdoc/>
        public override void Cleanup(
            object FileNode,
            object FileDesc,
            string FileName,
            uint Flags)
        {
            if (_specifics.Options.IsReadOnly)
                return;

            if ((Flags & CleanupDelete) == 0)
                return;

            string? pathToDelete = null;
            var storableType = StorableType.File;
            var handle = _handlesManager.GetHandle<IDisposable>(GetContextValue(FileDesc));

            // Extract path before disposing
            switch (handle)
            {
                case WinFspFileHandle fileHandle:
                {
                    pathToDelete = fileHandle.FileInfo.FullName;
                    storableType = StorableType.File;

                    break;
                }

                case WinFspDirectoryHandle dirHandle:
                {
                    pathToDelete = dirHandle.DirectoryInfo.FullName;
                    storableType = StorableType.Folder;

                    var directoryIdPath = Path.Combine(pathToDelete, FileSystem.Constants.Names.DIRECTORY_ID_FILENAME);
                    _specifics.DirectoryIdCache.CacheRemove(directoryIdPath);

                    break;
                }
            }

            // Close handle first
            CloseHandle(FileDesc);
            InvalidateContext(out FileDesc);
            Trace(STATUS_SUCCESS, FileName);

            try
            {
                if (pathToDelete is null)
                    return;

                // Then delete
                NativeRecycleBinHelpers.DeleteOrRecycle(pathToDelete, _specifics, storableType);
            }
            catch (Exception)
            {
                Trace(STATUS_UNSUCCESSFUL, FileName);
            }
        }

        /// <inheritdoc/>
        public override int CanDelete(
            object FileNode,
            object FileDesc,
            string FileName)
        {
            if (_specifics.Options.IsReadOnly)
                return Trace(STATUS_ACCESS_DENIED, FileName);

            switch (_handlesManager.GetHandle<IDisposable>(GetContextValue(FileDesc)))
            {
                case WinFspFileHandle fileHandle:
                {
                    if (PathHelpers.IsCoreName(fileHandle.FileInfo.Name))
                        return Trace(STATUS_ACCESS_DENIED, FileName);

                    return Trace(STATUS_SUCCESS, FileName);
                }

                case WinFspDirectoryHandle dirHandle:
                {
                    if (PathHelpers.IsCoreName(dirHandle.DirectoryInfo.Name))
                        return Trace(STATUS_ACCESS_DENIED, FileName);

                    var canDelete = true;
                    using var directoryEnumerator = dirHandle.DirectoryInfo.EnumerateFileSystemInfos().GetEnumerator();
                    while (directoryEnumerator.MoveNext())
                    {
                        // Check for any files except core files
                        canDelete &= PathHelpers.IsCoreName(directoryEnumerator.Current.Name);

                        // If the flag changed (directory is not empty), break the loop
                        if (!canDelete)
                            break;
                    }

                    if (!canDelete)
                        return Trace(STATUS_DIRECTORY_NOT_EMPTY, FileName);

                    return Trace(STATUS_SUCCESS, FileName);
                }

                default: return Trace(STATUS_ACCESS_DENIED, FileName);
            }
        }

        /// <inheritdoc/>
        public override int Rename(
            object FileNode,
            object FileDesc,
            string FileName,
            string NewFileName,
            bool ReplaceIfExists)
        {
            if (_specifics.Options.IsReadOnly)
                return Trace(STATUS_ACCESS_DENIED, FileName);

            var oldCiphertextPath = GetCiphertextPath(FileName);
            var newCiphertextPath = GetCiphertextPath(NewFileName);
            var isDirectory = IsDirectory(oldCiphertextPath);
            var newPathExists = isDirectory ? Directory.Exists(newCiphertextPath) : File.Exists(newCiphertextPath);

            try
            {
                if (!newPathExists)
                {
                    if (isDirectory)
                    {
                        Directory.Move(oldCiphertextPath, newCiphertextPath);
                    }
                    else
                    {
                        File.Move(oldCiphertextPath, newCiphertextPath);
                    }

                    return Trace(STATUS_SUCCESS, FileName);
                }
                else if (ReplaceIfExists)
                {
                    if (isDirectory)
                    {
                        // Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
                        return Trace(STATUS_ACCESS_DENIED, FileName);
                    }

                    File.Delete(newCiphertextPath);
                    File.Move(oldCiphertextPath, newCiphertextPath);

                    return Trace(STATUS_SUCCESS, FileName);
                }
                else
                    return Trace(STATUS_OBJECT_NAME_COLLISION, FileName);
            }
            catch (PathTooLongException)
            {
                return Trace(STATUS_NAME_TOO_LONG, FileName);
            }
            catch (FileNotFoundException)
            {
                return Trace(STATUS_OBJECT_NAME_NOT_FOUND, FileName);
            }
            catch (DirectoryNotFoundException)
            {
                return Trace(STATUS_OBJECT_PATH_NOT_FOUND, FileName);
            }
            catch (UnauthorizedAccessException)
            {
                return Trace(STATUS_ACCESS_DENIED, FileName);
            }
            catch (IOException ioEx)
            {
                if (ErrorHandlingHelpers.IsFileAlreadyExistsException(ioEx))
                    return Trace(STATUS_OBJECT_NAME_COLLISION, FileName);

                if (ErrorHandlingHelpers.IsDirectoryNotEmptyException(ioEx))
                    return Trace(STATUS_DIRECTORY_NOT_EMPTY, FileName);

                if (ErrorHandlingHelpers.IsDiskFullException(ioEx))
                    return Trace(STATUS_DISK_FULL, FileName);

                if (ErrorHandlingHelpers.Win32ErrorFromException(ioEx, out var win32error))
                    return Trace(NtStatusFromWin32(win32error), FileName);

                return Trace(STATUS_ACCESS_DENIED, FileName);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _specifics.Dispose();
            _handlesManager.Dispose();
        }
    }
}
