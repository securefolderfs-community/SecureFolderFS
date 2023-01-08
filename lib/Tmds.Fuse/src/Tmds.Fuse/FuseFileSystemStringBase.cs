using System;
using System.Runtime.InteropServices;
using System.Text;
using static Tmds.Linux.LibC;
using Tmds.Linux;

namespace Tmds.Fuse
{
    public class FuseFileSystemStringBase : FuseFileSystemBase
    {
        public static string RootPath => "/";

        private readonly Encoding _pathEncoding;

        public FuseFileSystemStringBase(Encoding encoding)
        {
            _pathEncoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        private unsafe string ToString(ReadOnlySpan<byte> span)
        {
            fixed (byte* ptr = span)
            {
                return _pathEncoding.GetString(ptr, span.Length);
            }
        }


        public override int Access(ReadOnlySpan<byte> path, mode_t mode)
            => Access(ToString(path), mode);

        protected virtual int Access(string path, mode_t mode)
            => -ENOSYS;

        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
            => ChMod(ToString(path), mode, fiRef);

        protected virtual int ChMod(string path, mode_t mode, FuseFileInfoRef fiRef)
            => -ENOSYS;

        public override int Chown(ReadOnlySpan<byte> path, uint uid, uint gid, FuseFileInfoRef fiRef)
            => Chown(ToString(path), uid, gid, fiRef);

        protected virtual int Chown(string path, uint uid, uint gid, FuseFileInfoRef fiRef)
            => -ENOSYS;

        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
            => Create(ToString(path), mode, ref fi);
        protected virtual int Create(string path, mode_t mode, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int FAllocate(ReadOnlySpan<byte> path, int mode, ulong offset, long length, ref FuseFileInfo fi)
            => FAllocate(ToString(path), mode, offset, length, ref fi);
        protected virtual int FAllocate(string path, int mode, ulong offset, long length, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int Flush(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
            => Flush(ToString(path), ref fi);
        protected virtual int Flush(string path, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int FSync(ReadOnlySpan<byte> path, bool onlyData, ref FuseFileInfo fi)
            => FSync(ToString(path), onlyData, ref fi);
        protected virtual int FSync(string path, bool onlyData, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
            => GetAttr(ToString(path), ref stat, fiRef);
        protected virtual int GetAttr(string path, ref stat stat, FuseFileInfoRef fiRef)
            => -ENOSYS;

        public override int GetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, Span<byte> data)
            => GetXAttr(ToString(path), ToString(name), data);
        protected virtual int GetXAttr(string path, string name, Span<byte> data)
            => -ENOSYS;

        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
            => Link(ToString(fromPath), ToString(toPath));
        protected virtual int Link(string fromPath, string toPath)
            => -ENOSYS;

        public override int ListXAttr(ReadOnlySpan<byte> path, Span<byte> list)
            => ListXAttr(ToString(path), list);
        protected virtual int ListXAttr(string path, Span<byte> list)
            => -ENOSYS;

        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
            => MkDir(ToString(path), mode);
        protected virtual int MkDir(string path, mode_t mode)
            => -ENOSYS;

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
            => Open(ToString(path), ref fi);
        protected virtual int Open(string path, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int OpenDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
            => OpenDir(ToString(path), ref fi);
        protected virtual int OpenDir(string path, ref FuseFileInfo fi)
            => 0;

        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
            => Read(ToString(path), offset, buffer, ref fi);
        protected virtual int Read(string path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
            => ReadDir(ToString(path), offset, flags, content, ref fi);
        protected virtual int ReadDir(string path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int ReadLink(ReadOnlySpan<byte> path, Span<byte> buffer)
            => ReadLink(ToString(path), buffer);
        protected virtual int ReadLink(string path, Span<byte> buffer)
            => -ENOSYS;

        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
            => Release(ToString(path), ref fi);
        protected virtual void Release(string path, ref FuseFileInfo fi)
        { }

        public override int ReleaseDir(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
            => ReleaseDir(ToString(path), ref fi);
        protected virtual int ReleaseDir(string path, ref FuseFileInfo fi)
            => -ENOSYS;

        public override int RemoveXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name)
            => RemoveXAttr(ToString(path), ToString(name));
        protected virtual int RemoveXAttr(string path, string name)
            => -ENOSYS;

        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
            => Rename(ToString(path), ToString(newPath), flags);
        protected virtual int Rename(string path, string newPath, int flags)
            => -ENOSYS;

        public override int RmDir(ReadOnlySpan<byte> path)
            => RmDir(ToString(path));
        protected virtual int RmDir(string path)
            => -ENOSYS;

        public override int SetXAttr(ReadOnlySpan<byte> path, ReadOnlySpan<byte> name, ReadOnlySpan<byte> data, int flags)
            => SetXAttr(ToString(path), ToString(name), data, flags);
        protected virtual int SetXAttr(string path, string name, ReadOnlySpan<byte> data, int flags)
            => -ENOSYS;

        public override int StatFS(ReadOnlySpan<byte> path, ref statvfs statfs)
            => StatFS(ToString(path), ref statfs);
        protected virtual int StatFS(string path, ref statvfs statfs)
            => -ENOSYS;

        public override int SymLink(ReadOnlySpan<byte> path, ReadOnlySpan<byte> target)
            => SymLink(ToString(path), ToString(target));
        protected virtual int SymLink(string path, string target)
            => -ENOSYS;

        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
            => Truncate(ToString(path), length, fiRef);
        protected virtual int Truncate(string path, ulong length, FuseFileInfoRef fiRef)
            => -ENOSYS;

        public override int Unlink(ReadOnlySpan<byte> path)
            => Unlink(ToString(path));
        protected virtual int Unlink(string path)
            => -ENOSYS;

        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
            => UpdateTimestamps(ToString(path), ref atime, ref mtime, fiRef);
        protected virtual int UpdateTimestamps(string path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
            => -ENOSYS;

        public override int Write(ReadOnlySpan<byte> path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi)
            => Write(ToString(path), off,  span, ref fi);
        protected virtual int Write(string path, ulong off, ReadOnlySpan<byte> span, ref FuseFileInfo fi)
            => -ENOSYS;
    }
}