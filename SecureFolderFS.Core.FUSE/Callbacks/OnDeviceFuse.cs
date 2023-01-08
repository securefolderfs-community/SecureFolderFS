using System.Text;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FUSE.OpenHandles;
using SecureFolderFS.Core.FUSE.UnsafeNative;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using Tmds.Fuse;
using Tmds.Linux;
using static Tmds.Linux.LibC;

// TODO Links
namespace SecureFolderFS.Core.FUSE.Callbacks
{
    internal sealed class OnDeviceFuse : BaseFuseCallbacks
    {
        public required ILocatableFolder LocatableContentFolder { get; init; }

        public required Security Security { get; init; }

        public required IDirectoryIdAccess DirectoryIdAccess { get; init; }

        public OnDeviceFuse(IPathConverter pathConverter, HandlesManager handlesManager)
            : base(pathConverter, handlesManager)
        {
        }

        public override unsafe int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            if (handlesManager.GetHandle<FuseFileHandle>(fiRef.Value.fh) == null)
                return -EBADF;

            var ciphertextPathPointer = GetCiphertextPathPointer(path);
            if (ciphertextPathPointer == null)
                return -ENOENT;

            return chmod(GetCiphertextPathPointer(path), mode);
        }

        public override unsafe int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            if (handlesManager.GetHandle<FuseFileHandle>(fiRef.Value.fh) == null)
                return -EBADF;

            var ciphertextPathPointer = GetCiphertextPathPointer(path);
            if (ciphertextPathPointer == null)
                return -ENOENT;

            return chown(GetCiphertextPathPointer(path), uid, gid);
        }

        public override unsafe int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            var fd = creat(GetCiphertextPathPointer(path), mode);
            if (fd == -1)
                return errno;

            close(fd);
            Open(path, ref fi);

            return 0;
        }

        public override unsafe int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath == null)
                return -ENOENT;

            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle == null || !handle.FileAccess.HasFlag(FileAccess.Write))
                return -EBADF;

            var ciphertextPathPointer = GetCiphertextPathPointer(path);
            var fd = open(ciphertextPathPointer, O_WRONLY);
            if (fd == -1)
                return errno;

            var result = fallocate(fd, mode, (long)offset, length);
            close(fd);

            return result;
        }

        public override unsafe int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (!path.SequenceEqual(RootPath) && !File.Exists(ciphertextPath) && !Directory.Exists(ciphertextPath))
                return -ENOENT;

            fixed (stat *statPtr = &stat)
            {
                var result = LibC.stat(GetCiphertextPathPointer(path), statPtr);
                if (File.Exists(ciphertextPath))
                    stat.st_size = Math.Max(0, Security.ContentCrypt.CalculateCleartextSize(stat.st_size - Security.HeaderCrypt.HeaderCiphertextSize));

                return result;
            }
        }

        public override unsafe int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath == null)
                return -ENOENT;

            if (File.Exists(ciphertextPath) || Directory.Exists(ciphertextPath))
                return -EEXIST;

            var result = mkdir(GetCiphertextPathPointer(path), mode);
            if (result != 0)
                return result;

            // Initialize directory with directory ID
            var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.DIRECTORY_ID_FILENAME);
            _ = DirectoryIdAccess.SetDirectoryId(directoryIdPath, DirectoryId.CreateNew());

            return 0;
        }

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath == null)
                return -ENOENT;

            var mode = FileMode.Open;
            if ((fi.flags & O_APPEND) != 0)
                mode = FileMode.Append;
            else if ((fi.flags & O_CREAT) != 0)
            {
                // TODO this breaks LibreOffice's lock files
                // if ((fi.flags & O_EXCL) != 0 && File.Exists(ciphertextPath))
                // return -EEXIST;

                // TODO Don't follow symlinks if O_EXCL is specified
                mode = FileMode.Create;
            }
            else if ((fi.flags & O_TRUNC) != 0)
                mode = FileMode.Truncate;

            // TODO Check if path is a symlink
            // if ((fi.flags & O_NOFOLLOW) != 0)
            // return -ELOOP;

            // Files are sometimes opened without an access flag, even though it should always be present.
            // Also the stream can't be write only for some reason.
            var access = (fi.flags & O_RDONLY) != 0 ? FileAccess.Read : FileAccess.ReadWrite;

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

            var handle = handlesManager.OpenHandleToFile(ciphertextPath, mode, access, FileShare.ReadWrite, options);
            if (handle == null)
                return -EACCES;

            fi.fh = handle.Value;
            return 0;
        }

        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath == null)
                return -ENOENT;

            if (File.Exists(ciphertextPath))
                return -ENOTDIR;

            fi.fh = handlesManager.OpenHandleToDirectory(ciphertextPath);
            return 0;
        }

        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle == null)
                return -EBADF;

            handle.Stream.Position = (long)offset;
            return handle.Stream.Read(buffer);
        }

        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath == null || handlesManager.GetHandle<DirectoryHandle>(fi.fh) == null)
                return -EBADF;

            content.AddEntry(".");
            content.AddEntry("..");

            foreach (var file in Directory.EnumerateFiles(ciphertextPath))
            {
                var name = Path.GetFileName(file);
                if (name != FileSystem.Constants.DIRECTORY_ID_FILENAME)
                    content.AddEntry(pathConverter.GetCleartextFileName(file));
            }

            foreach (var directory in Directory.EnumerateDirectories(ciphertextPath))
                content.AddEntry(pathConverter.GetCleartextFileName(directory));

            return 0;
        }

        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            handlesManager.CloseHandle(fi.fh);
        }

        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            if (handlesManager.GetHandle<DirectoryHandle>(fi.fh) == null)
                return -EBADF;

            handlesManager.CloseHandle(fi.fh);
            return 0;
        }

        public override unsafe int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            return UnsafeNativeApis.RenameAt2(0, GetCiphertextPathPointer(path), 0, GetCiphertextPathPointer(newPath), (uint)flags);
        }

        public override int RmDir(ReadOnlySpan<byte> path)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath == null)
                return -ENOENT;

            // TODO handle non-empty directories

            DirectoryIdAccess.RemoveDirectoryId(ciphertextPath);

            Directory.Delete(ciphertextPath, true);
            return 0;
        }

        public override unsafe int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            var ciphertextPathPointer = GetCiphertextPathPointer(path);
            if (ciphertextPathPointer == null)
                return -ENOENT;

            fixed (statvfs *statfsPtr = &statfs)
                return statvfs(ciphertextPathPointer, statfsPtr);
        }

        public override unsafe int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fiRef.Value.fh);
            if (handle == null || handle.FileAccess.HasFlag(FileAccess.Write))
                return -EBADF;

            var ciphertextPath = GetCiphertextPath(path);
            if (ciphertextPath == null)
                return -ENOENT;

            if (File.Exists(ciphertextPath))
                return -EISDIR;

            return truncate(GetCiphertextPathPointer(path), (long)length);
        }

        /// <remarks>
        /// This method is also responsible for file deletion.
        /// </remarks>
        public override unsafe int Unlink(ReadOnlySpan<byte> path)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (!File.Exists(ciphertextPath))
                return -ENOENT;
            if (Directory.Exists(ciphertextPath))
                return -EISDIR;

            var stat = new stat();
            LibC.stat(GetCiphertextPathPointer(path), &stat);

            if (S_ISLNK(stat.st_mode))
                return unlink(GetCiphertextPathPointer(path));

            File.Delete(ciphertextPath);
            return 0;
        }

        public override unsafe int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            var ciphertextPathPointer = GetCiphertextPathPointer(path);
            if (ciphertextPathPointer == null)
                return -EBADF;

            if (!fiRef.IsNull && handlesManager.GetHandle<FuseFileHandle>(fiRef.Value.fh) == null)
                return -EBADF;

            var fd = open(ciphertextPathPointer, O_WRONLY);
            if (fd == -1)
                return errno;

            fixed (timespec *times = new[] { atime, mtime })
            {
                var result = futimens(fd, times);
                close(fd);

                return result;
            }
        }

        public override int Write(ReadOnlySpan<byte> path, ulong offset, ReadOnlySpan<byte> buffer, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle == null)
                return -EBADF;

            handle.Stream.Position = (long)offset;
            handle.Stream.Write(buffer);

            return buffer.Length;
        }

        protected override unsafe string? GetCiphertextPath(ReadOnlySpan<byte> cleartextName)
        {
            fixed (byte *cleartextNamePtr = cleartextName)
            {
                var path = PathHelpers.PathFromVaultRoot(Encoding.UTF8.GetString(cleartextNamePtr, cleartextName.Length), LocatableContentFolder.Path);
                return pathConverter.ToCiphertext(path);
            }
        }
    }
}