using System;
using System.IO;
using System.Linq;
using System.Text;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native;
using SecureFolderFS.Core.FUSE.OpenHandles;
using SecureFolderFS.Core.FUSE.UnsafeNative;
using SecureFolderFS.Storage.Extensions;
using Tmds.Fuse;
using Tmds.Linux;
using static SecureFolderFS.Core.FUSE.UnsafeNative.UnsafeNativeApis;
using static Tmds.Linux.LibC;

namespace SecureFolderFS.Core.FUSE.Callbacks
{
    internal sealed class OnDeviceFuse : BaseFuseCallbacks
    {
        public override bool SupportsMultiThreading => true;

        public OnDeviceFuse(FileSystemSpecifics specifics, FuseHandlesManager handlesManager)
            : base(specifics, handlesManager)
        {
        }

        // Access doesn't need to be implemented due to the default_permissions mount option

        public override unsafe int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (chmod(ciphertextPathPtr, mode) == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (chown(ciphertextPathPtr, uid, gid) == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPathForUse(path);
            if (ciphertextPath is null)
                return -ENOENT;

            if ((fi.flags & O_CREAT) != 0 && (fi.flags & O_EXCL) != 0 && File.Exists(ciphertextPath))
                return -EEXIST;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                var fd = creat(ciphertextPathPtr, mode);
                if (fd == -1)
                    return -errno;

                close(fd);
                return Open(path, ref fi);
            }
        }

        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle is null || !handle.FileAccess.HasFlag(FileAccess.Write))
                return -EBADF;

            // Only the default mode (extend allocation and size) is supported.
            // FALLOC_FL_KEEP_SIZE and other flags arrive in 'mode', not in fi.flags
            if (mode != 0)
                return -EOPNOTSUPP;

            var newLength = (long)offset + length;
            lock (handle.Stream)
            {
                if (newLength > handle.Stream.Length)
                    handle.Stream.SetLength(newLength);
            }

            return 0;
        }

        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle is null)
                return -EBADF;

            // Flush is invoked for every close(2), including read-only descriptors
            if (!handle.FileAccess.HasFlag(FileAccess.Write))
                return 0;

            lock (handle.Stream)
                handle.Stream.Flush();

            return 0;
        }

        public override unsafe int FSync(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle is null)
                return -EBADF;

            // fsync(2) is valid on read-only descriptors - there is nothing to flush
            if (!handle.FileAccess.HasFlag(FileAccess.Write))
                return 0;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            lock (handle.Stream)
                handle.Stream.Flush();

            if (onlyData)
                return 0;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                var fd = open(ciphertextPathPtr, O_WRONLY);
                if (fd == -1)
                    return -errno;

                var result = fsync(fd);
                close(fd);
                if (result == -1)
                    return -errno;

                return 0;
            }
        }

        public override unsafe int FSyncDir(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            // Flush writable handles of files located inside the directory being synced
            foreach (var handle in handlesManager.OpenHandles)
            {
                if (handle is FuseFileHandle { Stream.CanWrite: true } fuseFileHandle && fuseFileHandle.Directory.StartsWith(ciphertextPath, StringComparison.Ordinal))
                {
                    lock (fuseFileHandle.Stream)
                        fuseFileHandle.Stream.Flush();
                }
            }

            if (onlyData)
                return 0;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                var fd = open(ciphertextPathPtr, O_RDONLY);
                if (fd == -1)
                    return -errno;

                var result = fsync(fd);
                close(fd);
                if (result == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            if (!path.SequenceEqual(RootPath) && !File.Exists(ciphertextPath) && !Directory.Exists(ciphertextPath))
                return -ENOENT;

            fixed (stat *statPtr = &stat)
            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (LibC.stat(ciphertextPathPtr, statPtr) == -1)
                    return -errno;

                // Prefer the open handle's stream length. It may include data that
                // has not been flushed to the ciphertext file yet
                if (!fiRef.IsNull && handlesManager.GetHandle<FuseFileHandle>(fiRef.Value.fh) is { } handle)
                {
                    lock (handle.Stream)
                        stat.st_size = handle.Stream.Length;
                }
                else if (File.Exists(ciphertextPath))
                {
                    stat.st_size = Math.Max(0, specifics.Security.ContentCrypt.CalculatePlaintextSize(stat.st_size - specifics.Security.HeaderCrypt.HeaderCiphertextSize));
                }

                return 0;
            }
        }

        public override unsafe int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> value)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte *namePtr = name)
            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                int result;
                if (value.Length == 0)
                    result = UnsafeNativeApis.GetXAttr(ciphertextPathPtr, namePtr, null, value.Length);
                else
                {
                    fixed (void *valuePtr = value)
                        result = UnsafeNativeApis.GetXAttr(ciphertextPathPtr, namePtr, valuePtr, value.Length);
                }

                if (result == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                int result;
                if (list.Length == 0)
                    result = UnsafeNativeApis.ListXAttr(ciphertextPathPtr, null, 0);
                else
                {
                    fixed (byte *listPtr = list)
                        result = UnsafeNativeApis.ListXAttr(ciphertextPathPtr, listPtr, list.Length);
                }

                if (result == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPathForUse(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (mkdir(ciphertextPathPtr, mode) == -1)
                    return -errno;
            }

            // Create new DirectoryID
            var directoryId = Guid.NewGuid().ToByteArray();
            var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.Names.DIRECTORY_ID_FILENAME);

            // Initialize directory with DirectoryID
            using var directoryIdStream = File.Create(directoryIdPath);
            directoryIdStream.Write(directoryId);

            // Set DirectoryID to known IDs
            specifics.DirectoryIdCache.CacheSet(directoryIdPath, new(directoryId));

            return 0;
        }

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            var mode = FileMode.Open;
            if ((fi.flags & O_APPEND) != 0)
                mode = FileMode.Append;
            else if ((fi.flags & O_TRUNC) != 0)
                mode = (fi.flags & O_CREAT) != 0 ? FileMode.Create : FileMode.Truncate;
            else if ((fi.flags & O_CREAT) != 0)
                mode = FileMode.OpenOrCreate; // O_CREAT without O_TRUNC must not truncate an existing file

            var options = FileOptions.None;
            if ((fi.flags & O_ASYNC) != 0)
                options |= FileOptions.Asynchronous;

            if ((fi.flags & O_DIRECT) != 0)
            {
                options |= FileOptions.WriteThrough;
                fi.direct_io = true;
            }

            if ((fi.flags & O_TMPFILE) != 0)
                options |= FileOptions.DeleteOnClose;

            var wantsWrite = (fi.flags & (O_WRONLY | O_RDWR)) != 0;
            if (FuseOptions!.IsReadOnly && (mode is FileMode.Create or FileMode.Truncate or FileMode.OpenOrCreate || wantsWrite))
                return -EROFS;

            // Read-only opens must not request write access. Otherwise, files with
            // read-only permissions (or on read-only media) cannot be opened at all
            var access = mode switch
            {
                FileMode.Append => FileAccess.Write,
                FileMode.Create or FileMode.Truncate => FileAccess.ReadWrite,
                _ => wantsWrite ? FileAccess.ReadWrite : FileAccess.Read
            };

            var handle = handlesManager.OpenFileHandle(ciphertextPath, mode, access, FileShare.ReadWrite, options);
            if (handle == FileSystem.Constants.INVALID_HANDLE)
                return -EACCES;

            fi.fh = handle;
            return 0;
        }

        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            if (File.Exists(ciphertextPath))
                return -ENOTDIR;

            if (!Directory.Exists(ciphertextPath))
                return -ENOENT;

            return 0;
        }

        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle is null)
                return -EBADF;

            // Lock on the handle's stream. The kernel can issue concurrent operations
            // on the same handle, and setting the position and reading must be atomic
            lock (handle.Stream)
            {
                if ((long)offset > handle.Stream.Length)
                    return 0;

                handle.Stream.Position = (long)offset;
                return handle.Stream.Read(buffer);
            }
        }

        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            content.AddEntry(".");
            content.AddEntry("..");

            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security);
            foreach (var entry in Directory.GetFileSystemEntries(ciphertextPath))
            {
                var ciphertextName = Path.GetFileName(entry);
                if (PathHelpers.IsCoreName(ciphertextName))
                    continue;

                // Skip entries whose names cannot be decrypted
                var plaintextName = NativePathHelpers.DecryptName(ciphertextName, ciphertextPath, specifics, directoryId);
                if (string.IsNullOrEmpty(plaintextName))
                    continue;

                content.AddEntry(plaintextName);
            }

            return 0;
        }

        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            handlesManager.CloseHandle(fi.fh);
        }

        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return 0;
        }

        public override unsafe int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte *namePtr = name)
            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (UnsafeNativeApis.RemoveXAttr(ciphertextPathPtr, namePtr) == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            var newCiphertextPath = GetCiphertextPathForUse(newPath);
            if (ciphertextPath is null || newCiphertextPath is null)
                return -ENOENT;

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            fixed (byte *newCiphertextPathPtr = Encoding.UTF8.GetBytes(newCiphertextPath))
            {
                if (RenameAt2(0, ciphertextPathPtr, 0, newCiphertextPathPtr, (uint)flags) == -1)
                    return -errno;
            }

            // Clean up old sidecar after successful rename
            NativePathHelpers.DeleteSidecarFile(
                Path.GetFileName(ciphertextPath),
                Path.GetDirectoryName(ciphertextPath) ?? string.Empty);

            return 0;
        }

        public override int RmDir(ReadOnlySpan<byte> path)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            // Protect core folders from deletion
            if (PathHelpers.IsCoreName(Path.GetFileName(Path.TrimEndingDirectorySeparator(ciphertextPath))))
                return -EACCES;

            if (Directory.EnumerateFileSystemEntries(ciphertextPath).Any(x => !PathHelpers.IsCoreName(Path.GetFileName(x))))
                return -ENOTEMPTY;

            var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.Names.DIRECTORY_ID_FILENAME);
            specifics.DirectoryIdCache.CacheRemove(directoryIdPath);

            if (FuseOptions.IsRecycleBinEnabled())
            {
                try
                {
                    // The folder is moved into the recycle bin together with its DirectoryID file
                    NativeRecycleBinHelpers.DeleteOrRecycle(ciphertextPath, specifics, StorableType.Folder);

                    // Clean up sidecar after successful delete/recycle
                    NativePathHelpers.DeleteSidecarFile(
                        Path.GetFileName(ciphertextPath),
                        Path.GetDirectoryName(ciphertextPath) ?? string.Empty);

                    return 0;
                }
                catch (FileNotFoundException)
                {
                    return -ENOENT;
                }
                catch (DirectoryNotFoundException)
                {
                    return -ENOENT;
                }
                catch (UnauthorizedAccessException)
                {
                    return -EACCES;
                }
                catch (IOException ioEx) when (ErrorHandlingHelpers.IsDiskFullException(ioEx))
                {
                    return -ENOSPC;
                }
                catch (Exception)
                {
                    return -EIO;
                }
            }

            // Read the DirectoryID so it can be restored if rmdir fails.
            // Deleting it permanently while the folder survives would make its contents undecryptable
            var directoryId = File.Exists(directoryIdPath) ? File.ReadAllBytes(directoryIdPath) : null;
            File.Delete(directoryIdPath);

            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (rmdir(ciphertextPathPtr) == -1)
                {
                    var error = errno;
                    if (directoryId is null)
                        return -error;

                    try
                    {
                        File.WriteAllBytes(directoryIdPath, directoryId);
                    }
                    catch (Exception)
                    {
                        // Best effort - the directory may have been removed concurrently
                    }

                    return -error;
                }
            }

            return 0;
        }

        public override unsafe int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> value, int flags)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte *namePtr = name)
            fixed (void *valuePtr = value)
            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (UnsafeNativeApis.SetXAttr(ciphertextPathPtr, namePtr, valuePtr, value.Length, flags) == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (statvfs *statfsPtr = &statfs)
            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (statvfs(ciphertextPathPtr, statfsPtr) == -1)
                    return -errno;
            }

            return 0;
        }

        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            if (Directory.Exists(ciphertextPath))
                return -EISDIR;

            if (!File.Exists(ciphertextPath))
                return -ENOENT;

            FuseFileHandle? handle;
            var temporaryHandleId = FileSystem.Constants.INVALID_HANDLE;
            if (fiRef.IsNull)
            {
                temporaryHandleId = handlesManager.OpenFileHandle(ciphertextPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None);
                if (temporaryHandleId == FileSystem.Constants.INVALID_HANDLE)
                    return -EIO;

                handle = handlesManager.GetHandle<FuseFileHandle>(temporaryHandleId)!;
            }
            else
            {
                handle = handlesManager.GetHandle<FuseFileHandle>(fiRef.Value.fh);
                if (handle is null || !handle.FileAccess.HasFlag(FileAccess.Write))
                    return -EBADF;
            }

            try
            {
                lock (handle.Stream)
                {
                    var position = handle.Stream.Position;
                    handle.Stream.SetLength((long)length);
                    handle.Stream.Position = position;
                }

                return 0;
            }
            finally
            {
                // Close the temporary handle
                if (temporaryHandleId != FileSystem.Constants.INVALID_HANDLE)
                    handlesManager.CloseHandle(temporaryHandleId);
            }
        }

        /// <remarks>
        /// This method is also responsible for file deletion.
        /// </remarks>
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (!File.Exists(ciphertextPath))
                return -ENOENT;

            if (Directory.Exists(ciphertextPath))
                return -EISDIR;

            // Protect core files from deletion
            if (PathHelpers.IsCoreName(Path.GetFileName(ciphertextPath)))
                return -EACCES;

            if (FuseOptions.IsRecycleBinEnabled())
            {
                try
                {
                    NativeRecycleBinHelpers.DeleteOrRecycle(ciphertextPath, specifics, StorableType.File);

                    // Clean up sidecar after successful delete/recycle
                    NativePathHelpers.DeleteSidecarFile(
                        Path.GetFileName(ciphertextPath),
                        Path.GetDirectoryName(ciphertextPath) ?? string.Empty);

                    return 0;
                }
                catch (FileNotFoundException)
                {
                    return -ENOENT;
                }
                catch (DirectoryNotFoundException)
                {
                    return -ENOENT;
                }
                catch (UnauthorizedAccessException)
                {
                    return -EACCES;
                }
                catch (IOException ioEx) when (ErrorHandlingHelpers.IsDiskFullException(ioEx))
                {
                    return -ENOSPC;
                }
                catch (Exception)
                {
                    return -EIO;
                }
            }

            return 0;
        }

        public override unsafe int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (timespec *times = new[] { atime, mtime })
            fixed (byte *ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (Directory.Exists(ciphertextPath))
                {
                    var fd = UnsafeNativeApis.OpenDir(ciphertextPathPtr);
                    if (fd is null)
                        return -errno;

                    var result = futimens(*(int*)fd, times);
                    CloseDir(fd);

                    if (result == -1)
                        return -errno;

                    return 0;
                }

                if (File.Exists(ciphertextPath))
                {
                    var fd = open(ciphertextPathPtr, O_WRONLY);
                    if (fd == -1)
                        return -errno;

                    var result = futimens(fd, times);
                    close(fd);

                    if (result == -1)
                        return -errno;

                    return 0;
                }

                return -ENOENT;
            }
        }

        public override int Write(ReadOnlySpan<byte> path, ulong offset, ReadOnlySpan<byte> buffer, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle is null || !handle.FileAccess.HasFlag(FileAccess.Write))
                return -EBADF;

            // Lock on the handle's stream. The kernel can issue concurrent operations
            // on the same handle, and setting the position and writing must be atomic
            lock (handle.Stream)
            {
                if (handle.FileMode == FileMode.Append)
                    offset = (ulong)handle.Stream.Length;

                // No SetLength needed here as the plaintext stream extends itself and
                // fills any gap with zeros when writing past the end of the file
                handle.Stream.Position = (long)offset;
                handle.Stream.Write(buffer);
            }

            return buffer.Length;
        }

        protected override unsafe string? GetCiphertextPath(ReadOnlySpan<byte> nativePlaintextName)
        {
            fixed (byte *plaintextNamePtr = nativePlaintextName)
            {
                var directoryId = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
                return NativePathHelpers.GetCiphertextPath(Encoding.UTF8.GetString(plaintextNamePtr, nativePlaintextName.Length), specifics, directoryId);
            }
        }

        private unsafe string? GetCiphertextPathForUse(ReadOnlySpan<byte> nativePlaintextName)
        {
            fixed (byte *plaintextNamePtr = nativePlaintextName)
            {
                var directoryId = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
                return NativePathHelpers.GetCiphertextPathForUse(Encoding.UTF8.GetString(plaintextNamePtr, nativePlaintextName.Length), specifics, directoryId);
            }
        }
    }
}