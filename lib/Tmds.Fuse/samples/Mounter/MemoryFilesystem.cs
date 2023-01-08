using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.IO;
using Tmds.Fuse;
using Tmds.Linux;
using static Tmds.Linux.LibC;

namespace Mounter
{
    class MemoryFileSystem : FuseFileSystemBase
    {
        struct EntryName : IEquatable<EntryName>
        {
            private byte[] _name;

            public EntryName(byte[] name)
                => _name = name;

            public bool Equals(EntryName other)
                => new Span<byte>(_name).SequenceEqual(other._name);

            public override bool Equals(object obj)
            {
                if (obj is EntryName name)
                {
                    return name.Equals(this);
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode() => Hash.GetFNVHashCode(_name);

            public static implicit operator EntryName(byte[] name) => new EntryName(name);

            public static implicit operator EntryName(ReadOnlySpan<byte> name) => new EntryName(name.ToArray());

            public static implicit operator ReadOnlySpan<byte>(EntryName name) => name._name;
        }

        class Entry
        {
            private uint _refCount;

            public uint RefCount => _refCount;
            public mode_t Mode { get; set; }
            public DateTime ATime { get; set; }
            public DateTime MTime { get; set; }

            public Entry()
                => _refCount = 1;

            public void RefCountInc()
            {
                if (_refCount == 0)
                {
                    throw new InvalidOperationException();
                }

                _refCount++;
            }

            public void RefCountDec()
            {
                if (_refCount == 0)
                {
                    throw new InvalidOperationException();
                }

                _refCount -= 1;

                if (_refCount == 0)
                {
                    DisposeEntry();
                }
            }

            protected virtual void DisposeEntry()
            { }
        }

        class File : Entry
        {
            private readonly Stream _content;

            public File(Stream content, mode_t mode)
            {
                _content = content;
                Mode = mode;
            }

            public int Size => (int)_content.Length;

            public int Read(ulong offset, Span<byte> buffer)
            {
                if (offset > (ulong)_content.Length)
                {
                    return 0;
                }
                _content.Position = (long)offset;
                return _content.Read(buffer);
            }

            public int Truncate(ulong length)
            {
                // Do we support this size?
                if (length > int.MaxValue)
                {
                    return -EINVAL;
                }

                _content.SetLength((long)length);

                return 0;
            }

            public int Write(ulong offset, ReadOnlySpan<byte> buffer)
            {
                // Do we support this size?
                ulong newLength = offset + (ulong)buffer.Length;
                if (newLength > int.MaxValue || offset > int.MaxValue)
                {
                    return -EFBIG;
                }

                // Copy the data
                _content.Position = (long)offset;
                _content.Write(buffer);
                return buffer.Length;
            }

            protected override void DisposeEntry()
            {
                _content.Dispose();
            }
        }

        class Directory : Entry
        {
            public Dictionary<EntryName, Entry> Entries { get; } = new Dictionary<EntryName, Entry>();

            public Directory(mode_t mode)
            {
                Mode = mode;
            }

            public Entry FindEntry(ReadOnlySpan<byte> path)
            {
                while (path.Length > 0 && path[0] == (byte)'/')
                {
                    path = path.Slice(1);
                }
                if (path.Length == 0)
                {
                    return this;
                }
                int endOfName = path.IndexOf((byte)'/');
                bool directChild = endOfName == -1;
                ReadOnlySpan<byte> name = directChild ? path : path.Slice(0, endOfName);
                if (Entries.TryGetValue(name, out Entry value))
                {
                    if (directChild)
                    {
                        return value;
                    }
                    else
                    {
                        Directory dir = value as Directory;
                        if (dir == null)
                        {
                            return null;
                        }
                        return dir.FindEntry(path.Slice(endOfName + 1));
                    }
                }
                else
                {
                    return null;
                }
            }

            public Directory AddDirectory(string name, mode_t mode)
                => AddDirectory(Encoding.UTF8.GetBytes(name), mode);

            public Directory AddDirectory(ReadOnlySpan<byte> name, mode_t mode)
            {
                Directory directory = null;
                try
                {
                    directory = new Directory(mode);
                    AddEntry(name, directory);
                    RefCountInc(); // subdirs link to their parent
                    return directory;
                }
                finally
                {
                    directory?.RefCountDec();
                }
            }

            public File AddFile(string name, string content, mode_t mode)
                => AddFile(Encoding.UTF8.GetBytes(name), Encoding.UTF8.GetBytes(content), mode);

            public File AddFile(ReadOnlySpan<byte> name, byte[] content, mode_t mode)
            {
                File file = null;
                try
                {
                    var memoryStream = new RecyclableMemoryStream(MemoryFileSystem.MemoryManager);
                    memoryStream.Write(content);
                    file = new File(memoryStream, mode);
                    AddEntry(name, file);
                    return file;
                }
                finally
                {
                    file?.RefCountDec();
                }
            }

            public void Remove(ReadOnlySpan<byte> name)
            {
                if (Entries.Remove(name, out Entry entry))
                {
                    entry.RefCountDec();
                    if (entry is Directory)
                    {
                        RefCountDec(); // subdirs link to their parent
                    }
                }
            }

            public void AddEntry(ReadOnlySpan<byte> name, Entry entry)
            {
                Entries.Add(name, entry);
                entry.RefCountInc();
            }

            // This is called when the FileSystem is Disposed
            protected void DisposeDirectory()
            {
                // Do a recursive 'remove' of all entries
                // causing memory to be returned to the MemoryManager.
                while (Entries.Count != 0)
                {
                    (EntryName name, Entry entry) = Entries.First();
                    if (entry is Directory dir)
                    {
                        dir.DisposeDirectory();
                    }
                    Remove(name);
                }
            }
        }

        class RootDirectory : Directory, IDisposable
        {
            public RootDirectory(mode_t mode) :
                base(mode)
            {}

            public void Dispose()
                => DisposeDirectory();
        }

        class OpenFile
        {
            private readonly File _file;

            public OpenFile(File file)
                => _file = file;

            public mode_t Mode
            {
                get => _file.Mode;
                set => _file.Mode = value;
            }

            public Entry Entry => _file;

            public int Read(ulong offset, Span<byte> buffer)
                => _file.Read(offset, buffer);

            public void Truncate(ulong offset)
                => _file.Truncate(offset);

            public int Write(ulong offset, ReadOnlySpan<byte> buffer)
                => _file.Write(offset, buffer);
        }

        public override void Dispose() => _root.Dispose();

        // TODO: inform fuse the implementation is not thread-safe.
        public MemoryFileSystem()
        {
            _root =  new RootDirectory(0b111_101_101);

            const int defaultFileMode = 0b100_100_100;  // r--r--r--
            const int defaultDirMode  = 0b111_101_101; // rwxr-xr-x
            // Add some stuff.
            _root.AddFile("file1", "Content of file1", defaultFileMode);
            Directory sampleDir = _root.AddDirectory("empty_dir", defaultDirMode);
            Directory dirWithFiles = _root.AddDirectory("dir_with_files", defaultDirMode);
            dirWithFiles.AddFile("file2", "Content of file2", defaultFileMode);
            dirWithFiles.AddFile("file3", "Content of file3", defaultFileMode);
            Directory nestedDir = dirWithFiles.AddDirectory("nested_dir", defaultDirMode);
            nestedDir.AddFile("file4", "Content of file4", defaultFileMode);
        }

        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            Entry entry = _root.FindEntry(path);
            if (entry == null)
            {
                return -ENOENT;
            }
            stat.st_atim = entry.ATime.ToTimespec();
            stat.st_mtim = entry.MTime.ToTimespec();
            stat.st_nlink = entry.RefCount;
            switch (entry)
            {
                case Directory directory:
                    stat.st_mode = S_IFDIR | entry.Mode;
                    stat.st_nlink++; // add additional link for self ('.')
                    break;
                case File f:
                    stat.st_mode = S_IFREG | entry.Mode;
                    stat.st_size = f.Size;
                    break;
            }
            return 0;
        }

        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
            => _openFiles[fi.fh].Read(offset, buffer);

        public override int Write(ReadOnlySpan<byte> path, ulong offset, ReadOnlySpan<byte> buffer, ref FuseFileInfo fi)
            => _openFiles[fi.fh].Write(offset, buffer);

        public override void Release(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
            => _openFiles.Remove(fi.fh);

        public override int Truncate(ReadOnlySpan<byte> path, ulong length, FuseFileInfoRef fiRef)
        {
            if (!fiRef.IsNull)
            {
                _openFiles[fiRef.Value.fh].Truncate(length);
                return 0;
            }
            else
            {
                Entry entry = _root.FindEntry(path);
                if (entry == null)
                {
                    return -ENOENT;
                }
                if (entry is File file)
                {
                    file.Truncate(length);
                    return 0;
                }
                else
                {
                    return -EISDIR;
                }
            }
        }

        public override int ChMod(ReadOnlySpan<byte> path, mode_t mode, FuseFileInfoRef fiRef)
        {
            if (!fiRef.IsNull)
            {
                _openFiles[fiRef.Value.fh].Mode = mode;
                return 0;
            }
            else
            {
                Entry entry = _root.FindEntry(path);
                if (entry == null)
                {
                    return -ENOENT;
                }
                entry.Mode = mode;
                return 0;
            }
        }

        public override int MkDir(ReadOnlySpan<byte> path, mode_t mode)
        {
            (Directory parent, bool parentIsNotDir, Entry entry) = FindParentAndEntry(path, out ReadOnlySpan<byte> name);
            if (parent == null)
            {
                return parentIsNotDir ? -ENOTDIR : -ENOENT;
            }
            if (entry != null)
            {
                return -EEXIST;
            }

            parent.AddDirectory(name, mode);

            return 0;
        }

        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            Entry entry = _root.FindEntry(path);
            if (entry == null)
            {
                return -ENOENT;
            }
            if (entry is Directory directory)
            {
                content.AddEntry(".");
                content.AddEntry("..");
                foreach (var child in directory.Entries)
                {
                    content.AddEntry(child.Key);
                }
                return 0;
            }
            else
            {
                return -ENOTDIR;
            }
        }

        public override int Create(ReadOnlySpan<byte> path, mode_t mode, ref FuseFileInfo fi)
        {
            (Directory parent, bool parentIsNotDir, Entry entry) = FindParentAndEntry(path, out ReadOnlySpan<byte> name);
            if (parent == null)
            {
                return parentIsNotDir ? -ENOTDIR : -ENOENT;
            }
            if (entry != null)
            {
                return -EEXIST;
            }

            File newFile = parent.AddFile(name, Array.Empty<byte>(), mode);
            fi.fh = FindFreeFileDescriptor(newFile);
            return 0;
        }

        public override int RmDir(ReadOnlySpan<byte> path)
        {
            (Directory parent, bool parentIsNotDir, Entry entry) = FindParentAndEntry(path, out ReadOnlySpan<byte> name);
            if (parent == null)
            {
                return parentIsNotDir ? -ENOTDIR : -ENOENT;
            }
            if (entry == null)
            {
                return -ENOENT;
            }

            if (entry is Directory dir)
            {
                if (dir.Entries.Count != 0)
                {
                    return -ENOTEMPTY;
                }

                parent.Remove(name);
                return 0;
            }
            else
            {
                return -ENOTDIR;
            }
        }

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            (Directory parent, bool parentIsNotDir, Entry entry) = FindParentAndEntry(path, out ReadOnlySpan<byte> name);
            if (parent == null)
            {
                return parentIsNotDir ? -ENOTDIR : -ENOENT;
            }

            if (entry == null)
            {
                return -ENOENT;
            }
            if (entry is File file)
            {
                if ((fi.flags & O_TRUNC) != 0)
                {
                    file.Truncate(0);
                }
                fi.fh = FindFreeFileDescriptor(file);
                return 0;
            }
            else
            {
                return -EISDIR;
            }
        }

        private ulong FindFreeFileDescriptor(File file)
        {
            for (uint i = 1; i < uint.MaxValue; i++)
            {
                if (!_openFiles.ContainsKey(i))
                {
                    _openFiles[i] = new OpenFile(file);
                    return i;
                }
            }
            return ulong.MaxValue;
        }

        public override int Unlink(ReadOnlySpan<byte> path)
        {
            (Directory parent, bool parentIsNotDir, Entry entry) = FindParentAndEntry(path, out ReadOnlySpan<byte> name);
            if (parent == null)
            {
                return parentIsNotDir ? -ENOTDIR : -ENOENT;
            }
            if (entry == null)
            {
                return -ENOENT;
            }

            if (entry is File file)
            {
                parent.Remove(name);
                return 0;
            }
            else
            {
                return -EISDIR;
            }
        }

        public override int Link(ReadOnlySpan<byte> fromPath, ReadOnlySpan<byte> toPath)
        {
            Entry from = _root.FindEntry(fromPath);
            if (from == null)
            {
                return -ENOENT;
            }
            (Directory parent, bool parentIsNotDir, Entry to) = FindParentAndEntry(toPath, out ReadOnlySpan<byte> name);
            if (parent == null)
            {
                return parentIsNotDir ? -ENOTDIR : -ENOENT;
            }
            if (to != null)
            {
                return -EEXIST;
            }
            parent.AddEntry(name, from);
            return 0;
        }

        public override int UpdateTimestamps(ReadOnlySpan<byte> path, ref timespec atime, ref timespec mtime, FuseFileInfoRef fiRef)
        {
            Entry entry;
            if (!fiRef.IsNull)
            {
                entry = _openFiles[fiRef.Value.fh].Entry;
            }
            else
            {
                entry = _root.FindEntry(path);
            }
            if (entry == null)
            {
                return -ENOENT;
            }
            DateTime now = atime.IsNow() || mtime.IsNow() ? DateTime.Now : DateTime.MinValue;
            if (!atime.IsOmit())
            {
                entry.ATime = atime.IsNow() ? now : atime.ToDateTime();
            }
            if (!mtime.IsOmit())
            {
                entry.MTime = mtime.IsNow() ? now : mtime.ToDateTime();
            }
            return 0;
        }

        public override int Rename(ReadOnlySpan<byte> path, ReadOnlySpan<byte> newPath, int flags)
        {
            System.Console.WriteLine(Encoding.UTF8.GetString(path) + " to " + Encoding.UTF8.GetString(newPath));
            return 0;
        }

        private (Directory parent, bool parentIsNotDir, Entry entry) FindParentAndEntry(ReadOnlySpan<byte> path, out ReadOnlySpan<byte> name)
        {
            SplitPathIntoParentAndName(path, out ReadOnlySpan<byte> parentDir, out name);
            Entry entry = _root.FindEntry(parentDir);
            Directory parent = entry as Directory;
            bool parentIsNotDir;
            if (parent != null)
            {
                parentIsNotDir = false;
                entry = parent.FindEntry(name);
            }
            else
            {
                parentIsNotDir = true;
                entry = null;
            }
            return (parent, parentIsNotDir, entry);
        }

        private void SplitPathIntoParentAndName(ReadOnlySpan<byte> path, out ReadOnlySpan<byte> parent, out ReadOnlySpan<byte> name)
        {
            int separatorPos = path.LastIndexOf((byte)'/');
            parent = path.Slice(0, separatorPos);
            name = path.Slice(separatorPos + 1);
        }

        internal static RecyclableMemoryStreamManager MemoryManager = new RecyclableMemoryStreamManager(); // can this one be cleaned up?
        private readonly RootDirectory _root;
        private readonly Dictionary<ulong, OpenFile> _openFiles = new Dictionary<ulong, OpenFile>();
    }
}