using System.Text;
using FuseSharp;
using FuseSharp.Native;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Native;
using SecureFolderFS.Core.MacFuse.OpenHandles;
using SecureFolderFS.Storage.Extensions;
using static FuseSharp.Native.LibC;

namespace SecureFolderFS.Core.MacFuse.Callbacks
{
    internal sealed class OnDeviceMacFuse : BaseMacFuseCallbacks
    {
        public override bool SupportsMultiThreading => true;

        public OnDeviceMacFuse(FileSystemSpecifics specifics, MacFuseHandlesManager handlesManager)
            : base(specifics, handlesManager)
        {
        }

        // Access doesn't need to be implemented due to the default_permissions mount option

        public override int ChMod(ReadOnlySpan<byte> path, uint mode, FuseFileInfoRef fiRef)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            try
            {
                File.SetUnixFileMode(ciphertextPath, (UnixFileMode)(mode & 0xFFFu));
                return 0;
            }
            catch (Exception ex)
            {
                return -MapExceptionToErrno(ex);
            }
        }

        public override unsafe int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (chown(ciphertextPathPtr, uid, gid) == -1)
                    return -errno;
            }

            return 0;
        }

        public override int Create(ReadOnlySpan<byte> path, uint mode, ref FuseFileInfo fi)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            var fileExists = File.Exists(ciphertextPath);
            if ((fi.flags & O_CREAT) != 0 && (fi.flags & O_EXCL) != 0 && fileExists)
                return -EEXIST;

            try
            {
                if (!fileExists)
                {
                    using (File.Create(ciphertextPath))
                    {
                    }

                    File.SetUnixFileMode(ciphertextPath, (UnixFileMode)(mode & 0xFFFu));
                }
            }
            catch (Exception ex)
            {
                return -MapExceptionToErrno(ex);
            }

            return Open(path, ref fi);
        }

        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            var handle = handlesManager.GetHandle<MacFuseFileHandle>(fi.fh);
            if (handle is null || !handle.FileAccess.HasFlag(FileAccess.Write))
                return -EBADF;

            // Only the default mode (extend allocation and size) is supported
            if (mode != 0)
                return -ENOTSUP;

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
            var handle = handlesManager.GetHandle<MacFuseFileHandle>(fi.fh);
            if (handle is null)
                return -EBADF;

            // Flush is invoked for every close(2), including read-only descriptors
            if (!handle.FileAccess.HasFlag(FileAccess.Write))
                return 0;

            lock (handle.Stream)
                handle.Stream.Flush();

            return 0;
        }

        public override int FSync(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<MacFuseFileHandle>(fi.fh);
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

            return FlushCiphertextToDisk(ciphertextPath);
        }

        public override int FSyncDir(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            // Flush writable handles of files located inside the directory being synced
            foreach (var handle in handlesManager.OpenHandles)
            {
                if (handle is MacFuseFileHandle { Stream.CanWrite: true } macFuseFileHandle && macFuseFileHandle.Directory.StartsWith(ciphertextPath, StringComparison.Ordinal))
                {
                    lock (macFuseFileHandle.Stream)
                        macFuseFileHandle.Stream.Flush();
                }
            }

            return 0;
        }

        public override int GetAttr(ReadOnlySpan<byte> path, ref Stat stat, FuseFileInfoRef fiRef)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            var isDirectory = Directory.Exists(ciphertextPath);
            var isFile = !isDirectory && File.Exists(ciphertextPath);
            if (!path.SequenceEqual(RootPath) && !isDirectory && !isFile)
                return -ENOENT;

            try
            {
                FileSystemInfo info = isFile ? new FileInfo(ciphertextPath) : new DirectoryInfo(ciphertextPath);

                stat.st_mode = (ushort)((isFile ? S_IFREG : S_IFDIR) | (ushort)File.GetUnixFileMode(ciphertextPath));
                stat.st_nlink = (ushort)(isFile ? 1 : 2);
                stat.st_uid = getuid();
                stat.st_gid = getgid();
                stat.st_atimespec = TimeSpec.FromDateTime(info.LastAccessTimeUtc);
                stat.st_mtimespec = TimeSpec.FromDateTime(info.LastWriteTimeUtc);
                stat.st_ctimespec = TimeSpec.FromDateTime(info.LastWriteTimeUtc);
                stat.st_birthtimespec = TimeSpec.FromDateTime(info.CreationTimeUtc);

                // Prefer the open handle's stream length. It may include data that
                // has not been flushed to the ciphertext file yet
                if (!fiRef.IsNull && handlesManager.GetHandle<MacFuseFileHandle>(fiRef.Value.fh) is { } handle)
                {
                    lock (handle.Stream)
                        stat.st_size = handle.Stream.Length;
                }
                else if (isFile)
                {
                    var ciphertextSize = ((FileInfo)info).Length;
                    stat.st_size = Math.Max(0, specifics.Security.ContentCrypt.CalculatePlaintextSize(ciphertextSize - specifics.Security.HeaderCrypt.HeaderCiphertextSize));
                }

                stat.st_blksize = 4096;
                stat.st_blocks = (stat.st_size + 511) / 512;

                return 0;
            }
            catch (Exception ex)
            {
                return -MapExceptionToErrno(ex);
            }
        }

        public override unsafe int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> value, uint position)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte* namePtr = NullTerminate(name))
            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                nint result;
                if (value.Length == 0)
                    result = getxattr(ciphertextPathPtr, namePtr, null, 0, position, 0);
                else
                {
                    fixed (void* valuePtr = value)
                        result = getxattr(ciphertextPathPtr, namePtr, valuePtr, (nuint)value.Length, position, 0);
                }

                if (result == -1)
                    return -errno;

                return (int)result;
            }
        }

        public override unsafe int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                nint result;
                if (list.Length == 0)
                    result = listxattr(ciphertextPathPtr, null, 0, 0);
                else
                {
                    fixed (byte* listPtr = list)
                        result = listxattr(ciphertextPathPtr, listPtr, (nuint)list.Length, 0);
                }

                if (result == -1)
                    return -errno;

                return (int)result;
            }
        }

        public override int MkDir(ReadOnlySpan<byte> path, uint mode)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            if (Directory.Exists(ciphertextPath) || File.Exists(ciphertextPath))
                return -EEXIST;

            try
            {
                Directory.CreateDirectory(ciphertextPath, (UnixFileMode)(mode & 0xFFFu));
            }
            catch (Exception ex)
            {
                return -MapExceptionToErrno(ex);
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

            if ((fi.flags & O_SYNC) != 0)
                options |= FileOptions.WriteThrough;

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
            var handle = handlesManager.GetHandle<MacFuseFileHandle>(fi.fh);
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

        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, DirectoryContent content, ref FuseFileInfo fi)
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

            fixed (byte* namePtr = NullTerminate(name))
            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (removexattr(ciphertextPathPtr, namePtr, 0) == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, uint flags)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            var newCiphertextPath = GetCiphertextPath(newPath);
            if (ciphertextPath is null || newCiphertextPath is null)
                return -ENOENT;

            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            fixed (byte* newCiphertextPathPtr = Encoding.UTF8.GetBytes(newCiphertextPath))
            {
                var result = flags == 0u
                    ? rename(ciphertextPathPtr, newCiphertextPathPtr)
                    : renamex_np(ciphertextPathPtr, newCiphertextPathPtr, flags);

                if (result == -1)
                    return -errno;
            }

            return 0;
        }

        public override int RmDir(ReadOnlySpan<byte> path)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            if (!Directory.Exists(ciphertextPath))
                return -ENOENT;

            // Protect core folders from deletion
            if (PathHelpers.IsCoreName(Path.GetFileName(Path.TrimEndingDirectorySeparator(ciphertextPath))))
                return -EACCES;

            if (Directory.EnumerateFileSystemEntries(ciphertextPath).Any(static x => !PathHelpers.IsCoreName(Path.GetFileName(x))))
                return -ENOTEMPTY;

            var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.Names.DIRECTORY_ID_FILENAME);
            try
            {
                if (FuseOptions.IsRecycleBinEnabled())
                    NativeRecycleBinHelpers.DeleteOrRecycle(ciphertextPath, specifics, StorableType.Folder);
                else
                    Directory.Delete(ciphertextPath, recursive: true); // Recursive because we want to delete the Directory ID file

                // Delete and remove Directory ID
                File.Delete(directoryIdPath);
                specifics.DirectoryIdCache.CacheRemove(directoryIdPath);

                return 0;
            }
            catch (Exception ex)
            {
                return -MapExceptionToErrno(ex);
            }
        }

        public override unsafe int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> value, int flags, uint position)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            // macFUSE forwards the kernel's internal VFS xattr flags (e.g. XATTR_NOSECURITY,
            // XATTR_NODEFAULT), but the setxattr(2) syscall only accepts XATTR_CREATE/XATTR_REPLACE
            // and rejects the rest with EINVAL. Re-issuing them verbatim on the backing file makes
            // Finder's FinderInfo write fail with error -50 and can leave copied files empty.
            var options = flags & (XATTR_CREATE | XATTR_REPLACE);

            fixed (byte* namePtr = NullTerminate(name))
            fixed (void* valuePtr = value)
            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (setxattr(ciphertextPathPtr, namePtr, valuePtr, (nuint)value.Length, position, options) == -1)
                    return -errno;
            }

            return 0;
        }

        public override unsafe int StatFS(ReadOnlySpan<byte> path, ref StatVfs statfs)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            fixed (StatVfs* statfsPtr = &statfs)
            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (statvfs(ciphertextPathPtr, statfsPtr) == -1)
                    return -errno;
            }

            return 0;
        }

        public override int Truncate(ReadOnlySpan<byte> path, long length, FuseFileInfoRef fiRef)
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

            MacFuseFileHandle? handle;
            var temporaryHandleId = FileSystem.Constants.INVALID_HANDLE;
            if (fiRef.IsNull)
            {
                temporaryHandleId = handlesManager.OpenFileHandle(ciphertextPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None);
                if (temporaryHandleId == FileSystem.Constants.INVALID_HANDLE)
                    return -EIO;

                handle = handlesManager.GetHandle<MacFuseFileHandle>(temporaryHandleId)!;
            }
            else
            {
                handle = handlesManager.GetHandle<MacFuseFileHandle>(fiRef.Value.fh);
                if (handle is null || !handle.FileAccess.HasFlag(FileAccess.Write))
                    return -EBADF;
            }

            try
            {
                lock (handle.Stream)
                {
                    var position = handle.Stream.Position;
                    handle.Stream.SetLength(length);
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
            if (ciphertextPath is null || !File.Exists(ciphertextPath))
                return -ENOENT;

            if (Directory.Exists(ciphertextPath))
                return -EISDIR;

            // Protect core files from deletion
            if (PathHelpers.IsCoreName(Path.GetFileName(ciphertextPath)))
                return -EACCES;

            try
            {
                if (FuseOptions.IsRecycleBinEnabled())
                    NativeRecycleBinHelpers.DeleteOrRecycle(ciphertextPath, specifics, StorableType.File);
                else
                    File.Delete(ciphertextPath);

                return 0;
            }
            catch (Exception ex)
            {
                return -MapExceptionToErrno(ex);
            }
        }

        public override unsafe int UpdateTimestamps(ReadOnlySpan<byte> path, ref TimeSpec atime, ref TimeSpec mtime, FuseFileInfoRef fiRef)
        {
            if (FuseOptions!.IsReadOnly)
                return -EROFS;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath is null)
                return -ENOENT;

            if (!File.Exists(ciphertextPath) && !Directory.Exists(ciphertextPath))
                return -ENOENT;

            var times = stackalloc TimeSpec[2] { atime, mtime };
            fixed (byte* ciphertextPathPtr = Encoding.UTF8.GetBytes(ciphertextPath))
            {
                if (utimensat(AT_FDCWD, ciphertextPathPtr, times, 0) == -1)
                    return -errno;
            }

            return 0;
        }

        public override int Write(ReadOnlySpan<byte> path, ulong offset, ReadOnlySpan<byte> buffer, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<MacFuseFileHandle>(fi.fh);
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

        protected override unsafe string? GetCiphertextPath(ReadOnlySpan<byte> plaintextName)
        {
            fixed (byte* plaintextNamePtr = plaintextName)
            {
                var directoryId = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
                return NativePathHelpers.GetCiphertextPath(Encoding.UTF8.GetString(plaintextNamePtr, plaintextName.Length), specifics, directoryId);
            }
        }

        /// <summary>
        /// Opens the ciphertext file and forces buffered data to permanent storage (F_FULLFSYNC).
        /// </summary>
        private static int FlushCiphertextToDisk(string ciphertextPath)
        {
            try
            {
                using var fileStream = new FileStream(ciphertextPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                fileStream.Flush(flushToDisk: true);
                return 0;
            }
            catch (Exception ex)
            {
                return -MapExceptionToErrno(ex);
            }
        }

        private static byte[] NullTerminate(ReadOnlySpan<byte> value)
        {
            var buffer = new byte[value.Length + 1];
            value.CopyTo(buffer);

            return buffer;
        }

        private static int MapExceptionToErrno(Exception exception)
        {
            return exception switch
            {
                FileNotFoundException or DirectoryNotFoundException => ENOENT,
                UnauthorizedAccessException => EACCES,
                PathTooLongException => ENAMETOOLONG,
                IOException => EIO,
                _ => EIO
            };
        }
    }
}
