using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Tmds.Linux;
using static Tmds.Linux.LibC;

namespace Tmds.Fuse
{
    class FuseMount : IFuseMount
    {
        private readonly string _mountPoint;
        private readonly MountOptions _mountOptions;
        private readonly IFuseFileSystem _fileSystem;
        private readonly getattr_Delegate _getattr;
        private readonly readdir_Delegate _readdir;
        private readonly open_Delegate _open;
        private readonly read_Delegate _read;
        private readonly release_Delegate _release;
        private readonly write_Delegate _write;
        private readonly unlink_Delegate _unlink;
        private readonly truncate_Delegate _truncate;
        private readonly rmdir_Delegate _rmdir;
        private readonly mkdir_Delegate _mkdir;
        private readonly create_Delegate _create;
        private readonly chmod_Delegate _chmod;
        private readonly link_Delegate _link;
        private readonly utimes_Delegate _utimens;
        private readonly readlink_Delegate _readlink;
        private readonly symlink_delegate _symlink;
        private readonly rename_delegate _rename;
        private readonly chown_delegate _chown;
        private readonly statfs_delegate _statfs;
        private readonly flush_delegate _flush;
        private readonly fsync_delegate _fsync;
        private readonly setxattr_delegate _setxattr;
        private readonly getxattr_delegate _getxattr;
        private readonly listxattr_delegate _listxattr;
        private readonly removeattr_delegate _removexattr;
        private readonly opendir_delegate _opendir;
        private readonly releasedir_delegate _releasedir;
        private readonly fsyncdir_delegate _fsyncdir;
        private readonly access_delegate _access;
        private readonly fallocate_delegate _fallocate;
        private readonly init_delegate _init;
        private Task _fuseLoopTask;
        private readonly object _gate = new object();
        private bool _mounted;
        private readonly TaskCompletionSource<object> _mountTaskCompletion = new TaskCompletionSource<object>();

        private unsafe class ManagedFiller
        {
            public readonly fuse_fill_dir* Filler;
            public readonly fuse_fill_dir_Delegate Delegate;

            public ManagedFiller(fuse_fill_dir* filler, fuse_fill_dir_Delegate fillDelegate)
            {
                Filler = filler;
                Delegate = fillDelegate;
            }
        }
        private ManagedFiller _previousFiller;

        public unsafe FuseMount(string mountPoint, IFuseFileSystem fileSystem, MountOptions options)
        {
            _mountPoint = mountPoint;
            _mountOptions = options;

            _fileSystem = fileSystem;
            _getattr = Getattr;
            _read = Read;
            _open = Open;
            _readdir = Readdir;
            _release = Release;
            _write = Write;
            _unlink = Unlink;
            _truncate = Truncate;
            _rmdir = Rmdir;
            _mkdir = Mkdir;
            _create = Create;
            _chmod = Chmod;
            _link = Link;
            _utimens = Utimens;
            _readlink = Readlink;
            _symlink = Symlink;
            _rename = Rename;
            _chown = Chown;
            _statfs = Statfs;
            _flush = Flush;
            _fsync = Fsync;
            _setxattr = Setxattr;
            _getxattr = Getxattr;
            _listxattr = Listxattr;
            _removexattr = Removexattr;
            _opendir = Opendir;
            _releasedir = Releasedir;
            _fsyncdir = Fsyncdir;
            _access = Access;
            _fallocate = Fallocate;
            _init = Init;
        }

        private void Init(IntPtr ptr, IntPtr ptr2)
        {
            _mountTaskCompletion.TrySetResult(null);
        }

        private unsafe int Fallocate(path* path, int mode, ulong offset, ulong length, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.FAllocate(ToSpan(path), mode, offset, (long)length, ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Access(path* path, mode_t mode)
        {
            try
            {
                return _fileSystem.Access(ToSpan(path), mode);
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Fsyncdir(path* path, int datasync, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.FSyncDir(ToSpan(path), datasync != 0, ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Releasedir(path* path, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.ReleaseDir(ToSpan(path), ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Opendir(path* path, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.OpenDir(ToSpan(path), ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Removexattr(path* path, void* name)
        {
            try
            {
                return _fileSystem.RemoveXAttr(ToSpan(path), ToSpan((path*)name));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Listxattr(path* path, void* buffer, UIntPtr size)
        {
            try
            {
                return _fileSystem.ListXAttr(ToSpan(path), new Span<byte>(buffer, (int)size));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Getxattr(path* path, void* name, void* buffer, UIntPtr size)
        {
            try
            {
                return _fileSystem.GetXAttr(ToSpan(path), ToSpan((path*)name), new Span<byte>(buffer, (int)size));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Setxattr(path* path, void* name, void* buffer, UIntPtr size, int flags)
        {
            try
            {
                return _fileSystem.SetXAttr(ToSpan(path), ToSpan((path*)name), new ReadOnlySpan<byte>(buffer, (int)size), flags);
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Fsync(path* path,int datasync, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.FSync(ToSpan(path), datasync != 0, ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Flush(path* path, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.Flush(ToSpan(path), ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Statfs(path* path, statvfs* vfs)
        {
            try
            {
                Span<statvfs> span = new Span<statvfs>(vfs, 1);
                span.Clear();
                return _fileSystem.StatFS(ToSpan(path), ref MemoryMarshal.GetReference(span));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Chown(path* path, uint uid, uint gid, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.Chown(ToSpan(path), uid, gid, ToFileInfo(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Rename(path* path, path* path2, int flags)
        {
            try
            {
                return _fileSystem.Rename(ToSpan(path), ToSpan(path2), flags);
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Symlink(path* path, path* path2)
        {
            try
            {
                return _fileSystem.SymLink(ToSpan(path2), ToSpan(path));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Readlink(path* path, void* buffer, UIntPtr size)
        {
            try
            {
                return _fileSystem.ReadLink(ToSpan(path), new Span<byte>(buffer, (int)size));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Utimens(path* path, timespec* tv, fuse_file_info* fi)
        {
            try
            {
                Span<timespec> specs = new Span<timespec>(tv, 2);
                return _fileSystem.UpdateTimestamps(ToSpan(path),
                                                    ref MemoryMarshal.GetReference(specs),
                                                    ref MemoryMarshal.GetReference(specs.Slice(1)),
                                                    ToFileInfo(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Link(path* fromPath, path* toPath)
        {
            try
            {
                return _fileSystem.Link(ToSpan(fromPath), ToSpan(toPath));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Chmod(path* path, mode_t mode, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.ChMod(ToSpan(path), mode, ToFileInfo(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Truncate(path* path, ulong length, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.Truncate(ToSpan(path), length, ToFileInfo(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Create(path* path, mode_t mode, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.Create(ToSpan(path), mode, ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Mkdir(path* path, mode_t mode)
        {
            try
            {
                return _fileSystem.MkDir(ToSpan(path), mode);
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Rmdir(path* path)
        {
            try
            {
                return _fileSystem.RmDir(ToSpan(path));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Unlink(path* path)
        {
            try
            {
                return _fileSystem.Unlink(ToSpan(path));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Write(path* path, void* buffer, UIntPtr size, ulong off, fuse_file_info* fi)
        {
            try
            {
                // TODO: handle size > int.MaxValue
                return _fileSystem.Write(ToSpan(path), off, new ReadOnlySpan<byte>(buffer, (int)size), ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Getattr(path* path, stat* stat, fuse_file_info* fi)
        {
            try
            {
                Span<stat> span = new Span<stat>(stat, 1);
                span.Clear();
                return _fileSystem.GetAttr(ToSpan(path), ref MemoryMarshal.GetReference(span), ToFileInfo(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Readdir(path* path, void* buf, fuse_fill_dir* filler, ulong offset, fuse_file_info* fi, int flags)
        {
            try
            {
                fuse_fill_dir_Delegate fillDelegate;
                ManagedFiller previousFiller = _previousFiller;
                if (previousFiller != null && previousFiller.Filler == filler)
                {
                    fillDelegate = previousFiller.Delegate;
                }
                else
                {
                    fillDelegate = Marshal.GetDelegateForFunctionPointer<fuse_fill_dir_Delegate>(new IntPtr(filler));
                    _previousFiller = new ManagedFiller(filler, fillDelegate);
                }

                return _fileSystem.ReadDir(ToSpan(path), offset, (ReadDirFlags)flags, ToDirectoryContent(buf, fillDelegate), ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Open(path* path, fuse_file_info* fi)
        {
            try
            {
                return _fileSystem.Open(ToSpan(path), ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Read(path* path, void* buffer, UIntPtr size, ulong off, fuse_file_info* fi)
        {
            try
            {
                // TODO: handle size > int.MaxValue
                return _fileSystem.Read(ToSpan(path), off, new Span<byte>(buffer, (int)size), ref ToFileInfoRef(fi));
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe int Release(path* path, fuse_file_info* fi)
        {
            try
            {
                _fileSystem.Release(ToSpan(path), ref ToFileInfoRef(fi));
                return 0;
            }
            catch
            {
                return -EIO;
            }
        }

        private unsafe FuseFileInfoRef ToFileInfo(fuse_file_info* fi)
        {
            if (fi == null)
            {
                return new FuseFileInfoRef(new Span<FuseFileInfo>());
            }
            else
            {
                return new FuseFileInfoRef(new Span<FuseFileInfo>(fi, 1));
            }
        }

        private unsafe ref FuseFileInfo ToFileInfoRef(fuse_file_info* fi)
        {
            if (fi == null)
            {
                throw new InvalidOperationException($"unexpected: {nameof(fi)} is null");
            }
            else
            {
                return ref MemoryMarshal.GetReference<FuseFileInfo>(new Span<FuseFileInfo>(fi, 1));
            }
        }

        private unsafe ReadOnlySpan<byte> ToSpan(path* path)
        {
            var span = new ReadOnlySpan<byte>(path, int.MaxValue);
            return span.Slice(0, span.IndexOf((byte)0));
        }

        private unsafe DirectoryContent ToDirectoryContent(void* buffer, fuse_fill_dir_Delegate fillDelegate) => new DirectoryContent(buffer, fillDelegate);

        public unsafe void Mount()
        {
            lock (_gate)
            {
                try
                {
                    _fuseLoopTask = Task.Factory.StartNew(() =>
                    {
                        if (!LibFuse.IsAvailable)
                        {
                            throw new FuseException($"libfuse({LibFuse.LibraryName}) is not available on this system.");
                        }

                        fuse_args args;
                        fuse* fuse = null;
                        try
                        {
                            LibFuse.fuse_opt_add_arg(&args, "");

                            fuse_operations ops;
                            ops.getattr = Marshal.GetFunctionPointerForDelegate(_getattr);
                            ops.readdir = Marshal.GetFunctionPointerForDelegate(_readdir);
                            ops.open = Marshal.GetFunctionPointerForDelegate(_open);
                            ops.read = Marshal.GetFunctionPointerForDelegate(_read);
                            ops.release = Marshal.GetFunctionPointerForDelegate(_release);
                            ops.write = Marshal.GetFunctionPointerForDelegate(_write);
                            ops.unlink = Marshal.GetFunctionPointerForDelegate(_unlink);
                            ops.truncate = Marshal.GetFunctionPointerForDelegate(_truncate);
                            ops.rmdir = Marshal.GetFunctionPointerForDelegate(_rmdir);
                            ops.mkdir = Marshal.GetFunctionPointerForDelegate(_mkdir);
                            ops.create = Marshal.GetFunctionPointerForDelegate(_create);
                            ops.chmod = Marshal.GetFunctionPointerForDelegate(_chmod);
                            ops.link = Marshal.GetFunctionPointerForDelegate(_link);
                            ops.utimens = Marshal.GetFunctionPointerForDelegate(_utimens);
                            ops.readlink = Marshal.GetFunctionPointerForDelegate(_readlink);
                            ops.symlink = Marshal.GetFunctionPointerForDelegate(_symlink);
                            ops.rename = Marshal.GetFunctionPointerForDelegate(_rename);
                            ops.chown = Marshal.GetFunctionPointerForDelegate(_chown);
                            ops.statfs = Marshal.GetFunctionPointerForDelegate(_statfs);
                            ops.flush = Marshal.GetFunctionPointerForDelegate(_flush);
                            ops.fsync = Marshal.GetFunctionPointerForDelegate(_fsync);
                            ops.setxattr = Marshal.GetFunctionPointerForDelegate(_setxattr);
                            ops.getxattr = Marshal.GetFunctionPointerForDelegate(_getxattr);
                            ops.listxattr = Marshal.GetFunctionPointerForDelegate(_listxattr);
                            ops.removexattr = Marshal.GetFunctionPointerForDelegate(_removexattr);
                            ops.opendir = Marshal.GetFunctionPointerForDelegate(_opendir);
                            ops.releasedir = Marshal.GetFunctionPointerForDelegate(_releasedir);
                            ops.fsyncdir = Marshal.GetFunctionPointerForDelegate(_fsyncdir);
                            ops.access = Marshal.GetFunctionPointerForDelegate(_access);
                            ops.fallocate = Marshal.GetFunctionPointerForDelegate(_fallocate);
                            ops.init = Marshal.GetFunctionPointerForDelegate(_init);

                            fuse = LibFuse.fuse_new(&args, &ops, (UIntPtr)sizeof(fuse_operations), null);
                            if (fuse == null)
                            {
                                throw CreateException(nameof(LibFuse.fuse_new), 0);
                            }
                            int rv = LibFuse.fuse_mount(fuse, _mountPoint);
                            if (rv != 0)
                            {
                                throw CreateException(nameof(LibFuse.fuse_mount), rv);
                            }
                            try
                            {
                                bool singleThread = !_fileSystem.SupportsMultiThreading || _mountOptions.SingleThread;
                                if (singleThread)
                                {
                                    rv = LibFuse.fuse_loop(fuse);
                                }
                                else
                                {
                                    rv = LibFuse.fuse_loop_mt(fuse, clone_fd: 0);
                                }
                                if (rv == 0)
                                {
                                    _mountTaskCompletion.TrySetResult(null);
                                }
                                else
                                {
                                    _mountTaskCompletion.TrySetException(CreateException(nameof(LibFuse.fuse_loop), rv));
                                }
                            }
                            catch (Exception e)
                            {
                                _mountTaskCompletion.TrySetException(e);
                            }
                            finally
                            {
                                lock (_gate)
                                {
                                    LibFuse.fuse_unmount(fuse);
                                    if (_mounted)
                                    {
                                        _mounted = false;
                                        _fileSystem.Dispose();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _mountTaskCompletion.TrySetException(e);
                        }
                        finally
                        {
                            if (fuse != null)
                            {
                                LibFuse.fuse_destroy(fuse);
                            }
                            LibFuse.fuse_opt_free_args(&args);
                        }
                    }, TaskCreationOptions.LongRunning);

                    _mountTaskCompletion.Task.GetAwaiter().GetResult();
                    _mounted = true;
                }
                finally
                {
                    if (!_mounted)
                    {
                        _fileSystem.Dispose();
                    }
                }
            }
        }

        private Exception CreateException(string operation, int returnValue)
        {
            return new FuseException($"Failed to {operation}, the function returned {returnValue}.");
        }

        public Task WaitForUnmountAsync()
            => _fuseLoopTask;

        public unsafe void LazyUnmount()
        {
            lock (_gate)
            {
                if (_mounted)
                {
                    Fuse.LazyUnmount(_mountPoint);

                    // Give fuse a kick in order to help unblock the loop
                    File.GetLastAccessTime(_mountPoint);
                }
            }
        }

        public async Task<bool> UnmountAsync(int millisecondsTimeout)
        {
            LazyUnmount();
            if (_fuseLoopTask.IsCompleted)
            {
                return true;
            }
            if (millisecondsTimeout < 0)
            {
                await _fuseLoopTask;
                return true;
            }
            else if (millisecondsTimeout > 0)
            {
                await Task.WhenAny(_fuseLoopTask, Task.Delay(millisecondsTimeout));
            }
            return _fuseLoopTask.IsCompleted;
        }

        public void Dispose()
        {
            LazyUnmount();
        }
    }
}