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

// TODO More error handling
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
            return chmod(GetCiphertextPathPointer(path), mode);
        }

        public override unsafe int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
        {
            return chown(GetCiphertextPathPointer(path), uid, gid);
        }

        public override unsafe int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            creat(GetCiphertextPathPointer(path), mode);
            Open(path, ref fi);

            return 0;
        }

        public override unsafe int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            var ciphertextPath = GetCiphertextPath(path);
            var mode = FileMode.Open;

            // https://man7.org/linux/man-pages/man2/open.2.html
            if ((fi.flags & O_APPEND) != 0)
                mode = FileMode.Append;
            else if ((fi.flags & O_CREAT) != 0)
                mode = FileMode.Create;
            else if ((fi.flags & O_TRUNC) != 0)
                mode = FileMode.Truncate;

            // TODO file share and options
            var handle = handlesManager.OpenHandleToFile(ciphertextPath, mode, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None);
            if (handle == null)
                return -EACCES;

            fi.fh = handle.Value;
            return 0;
        }

        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            content.AddEntry(".");
            content.AddEntry("..");

            var ciphertextPath = GetCiphertextPath(path);
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

        public override unsafe int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            return UnsafeNativeApis.RenameAt2(0, GetCiphertextPathPointer(path), 0, GetCiphertextPathPointer(newPath), (uint)flags);
        }

        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);

            handle.Stream.Position = (long)offset;
            return handle.Stream.Read(buffer);
        }

        public override int Write(ReadOnlySpan<byte> path, ulong offset, ReadOnlySpan<byte> buffer, ref FuseFileInfo fi)
        {
            var handle = handlesManager.GetHandle<FuseFileHandle>(fi.fh);
            if (handle == null)
                return -EBADF; // Invalid file handle

            handle.Stream.Position = (long)offset;
            handle.Stream.Write(buffer);

            return buffer.Length;
        }

        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            handlesManager.CloseHandle(fi.fh);
        }

        /// <remarks>
        /// This method is also responsible for file deletion.
        /// </remarks>
        public override int Unlink(ReadOnlySpan<byte> path)
        {
            var ciphertextPath = GetCiphertextPath(path);
            if (!File.Exists(ciphertextPath))
                return -ENOENT;
            if (Directory.Exists(ciphertextPath))
                return -EISDIR;

            File.Delete(ciphertextPath);
            return 0;
        }

        public override unsafe int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            return truncate(GetCiphertextPathPointer(path), (long)length);
        }

        public override unsafe int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            var ciphertextPathPointer = GetCiphertextPathPointer(path);

            var fd = open(ciphertextPathPointer, O_WRONLY);
            fixed (timespec * times = new[] { atime, mtime })
            {
                var result = futimens(fd, times);
                close(fd);

                return result;
            }
        }

        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            return 0;
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
            if (Directory.Exists(ciphertextPath))
                return -EEXIST;

            var result = mkdir(GetCiphertextPathPointer(path), mode);

            // Initialize directory with directory ID
            var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.DIRECTORY_ID_FILENAME);
            _ = DirectoryIdAccess.SetDirectoryId(directoryIdPath, DirectoryId.CreateNew());

            return result;
        }

        public override unsafe int RmDir(ReadOnlySpan<byte> path)
        {
            var ciphertextPath = GetCiphertextPath(path);
            DirectoryIdAccess.RemoveDirectoryId(ciphertextPath);

            Directory.Delete(ciphertextPath, true);
            return 0;
        }

        public override unsafe int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
        {
            fixed (statvfs *statfsPtr = &statfs)
                return statvfs(GetCiphertextPathPointer(path), statfsPtr);
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