using System;
using Tmds.Linux;

namespace Tmds.Fuse
{
    using size_t = System.UIntPtr;

    struct fuse { }
    struct path { }
    struct fuse_fill_dir { }
    enum fuse_fill_dir_flags { }
    struct fuse_file_info { }

    unsafe struct fuse_args
    {
        /** Argument count */
        int argc;

        /** Argument vector.  NULL terminated */
        byte** argv;

        /** Is 'argv' allocated? */
        int allocated;
    };

    unsafe delegate int fuse_fill_dir_Delegate(void* buf, void* name, stat* stat, ulong off, fuse_fill_dir_flags flags);
    unsafe delegate int getattr_Delegate(path* path, stat* stat, fuse_file_info* fi);
    unsafe delegate int readdir_Delegate(path* path, void* buf, fuse_fill_dir* filler, ulong offset, fuse_file_info* fi, int flags);
    unsafe delegate int open_Delegate(path* path, fuse_file_info* fi);
    unsafe delegate int read_Delegate(path* path, void* buffer, size_t size, ulong off, fuse_file_info* fi);
    unsafe delegate int release_Delegate(path* path, fuse_file_info* fi);
    unsafe delegate int write_Delegate(path* path, void* buffer, size_t size, ulong off, fuse_file_info* fi);
    unsafe delegate int unlink_Delegate(path* path);
    unsafe delegate int truncate_Delegate(path* path, ulong length, fuse_file_info* fi);
    unsafe delegate int rmdir_Delegate(path* path);
    unsafe delegate int mkdir_Delegate(path* path, mode_t mode);
    unsafe delegate int create_Delegate(path* path, mode_t mode, fuse_file_info* fi);
    unsafe delegate int chmod_Delegate(path* path, mode_t mode, fuse_file_info* fi);
    unsafe delegate int link_Delegate(path* fromPath, path* toPath);
    unsafe delegate int utimes_Delegate(path* path, timespec* tv, fuse_file_info* fi);
    unsafe delegate int readlink_Delegate(path* path, void* buffer, size_t size);
    unsafe delegate int symlink_delegate(path* path, path* path2);
    unsafe delegate int rename_delegate(path* path, path* path2, int flags);
    unsafe delegate int chown_delegate(path* path, uint uid, uint gid, fuse_file_info* fi);
    unsafe delegate int statfs_delegate(path* path, statvfs* vfs);
    unsafe delegate int flush_delegate(path* path, fuse_file_info* fi);
    unsafe delegate int fsync_delegate(path* path, int datasync, fuse_file_info* fi);
    unsafe delegate int setxattr_delegate(path* path, void* name, void* buffer, size_t size, int flags);
    unsafe delegate int getxattr_delegate(path* path, void* name, void* buffer, size_t size);
    unsafe delegate int listxattr_delegate(path* path, void* buffer, size_t size);
    unsafe delegate int removeattr_delegate(path* path, void* name);
    unsafe delegate int opendir_delegate(path* path, fuse_file_info* fi);
    unsafe delegate int releasedir_delegate(path* path, fuse_file_info* fi);
    unsafe delegate int fsyncdir_delegate(path* path, int datasync, fuse_file_info* fi);
    unsafe delegate int access_delegate(path* path, mode_t mode);
    unsafe delegate int fallocate_delegate(path* path, int mode, ulong off, ulong length, fuse_file_info* fi);
    unsafe delegate void init_delegate(IntPtr ptr, IntPtr ptr2);
    /**
    * The file system operations:
    *
    * Most of these should work very similarly to the well known UNIX
    * file system operations.  A major exception is that instead of
    * returning an error in 'errno', the operation should return the
    * negated error value (-errno) directly.
    *
    * All methods are optional, but some are essential for a useful
    * filesystem (e.g. getattr).  Open, flush, release, fsync, opendir,
    * releasedir, fsyncdir, access, create, truncate, lock, init and
    * destroy are special purpose methods, without which a full featured
    * filesystem can still be implemented.
    *
    * In general, all methods are expected to perform any necessary
    * permission checking. However, a filesystem may delegate this task
    * to the kernel by passing the `default_permissions` mount option to
    * `fuse_new()`. In this case, methods will only be called if
    * the kernel's permission check has succeeded.
    *
    * Almost all operations take a path which can be of any length.
    */
    struct fuse_operations
    {
        /** Get file attributes.
        *
        * Similar to stat().  The 'st_dev' and 'st_blksize' fields are
        * ignored. The 'st_ino' field is ignored except if the 'use_ino'
        * mount option is given. In that case it is passed to userspace,
        * but libfuse and the kernel will still assign a different
        * inode for internal use (called the "nodeid").
        *
        * `fi` will always be NULL if the file is not currenly open, but
        * may also be NULL if the file is open.
        */
        //int (*getattr) (const char *, struct stat *, struct fuse_file_info *fi);
        public IntPtr getattr;

        /** Read the target of a symbolic link
        *
        * The buffer should be filled with a null terminated string.  The
        * buffer size argument includes the space for the terminating
        * null character.	If the linkname is too long to fit in the
        * buffer, it should be truncated.	The return value should be 0
        * for success.
        */
        //int (*readlink) (const char *, char *, size_t);
        public IntPtr readlink;

        /** Create a file node
        *
        * This is called for creation of all non-directory, non-symlink
        * nodes.  If the filesystem defines a create() method, then for
        * regular files that will be called instead.
        */
        //int (*mknod) (const char *, mode_t, dev_t);
        IntPtr mknod;

        /** Create a directory
        *
        * Note that the mode argument may not have the type specification
        * bits set, i.e. S_ISDIR(mode) can be false.  To obtain the
        * correct directory type bits use  mode|S_IFDIR
        * */
        //int (*mkdir) (const char *, mode_t);
        public IntPtr mkdir;

        /** Remove a file */
        //int (*unlink) (const char *);
        public IntPtr unlink;

        /** Remove a directory */
        //int (*rmdir) (const char *);
        public IntPtr rmdir;

        /** Create a symbolic link */
        //int (*symlink) (const char *, const char *);
        public IntPtr symlink;

        /** Rename a file
        *
        * *flags* may be `RENAME_EXCHANGE` or `RENAME_NOREPLACE`. If
        * RENAME_NOREPLACE is specified, the filesystem must not
        * overwrite *newname* if it exists and return an error
        * instead. If `RENAME_EXCHANGE` is specified, the filesystem
        * must atomically exchange the two files, i.e. both must
        * exist and neither may be deleted.
        */
        //int (*rename) (const char *, const char *, unsigned int flags);
        public IntPtr rename;

        /** Create a hard link to a file */
        //int (*link) (const char *, const char *);
        public IntPtr link;

        /** Change the permission bits of a file
        *
        * `fi` will always be NULL if the file is not currenly open, but
        * may also be NULL if the file is open.
        */
        //int (*chmod) (const char *, mode_t, struct fuse_file_info *fi);
        public IntPtr chmod;

        /** Change the owner and group of a file
        *
        * `fi` will always be NULL if the file is not currenly open, but
        * may also be NULL if the file is open.
        *
        * Unless FUSE_CAP_HANDLE_KILLPRIV is disabled, this method is
        * expected to reset the setuid and setgid bits.
        */
        //int (*chown) (const char *, uid_t, gid_t, struct fuse_file_info *fi);
        public IntPtr chown;

        /** Change the size of a file
        *
        * `fi` will always be NULL if the file is not currenly open, but
        * may also be NULL if the file is open.
        *
        * Unless FUSE_CAP_HANDLE_KILLPRIV is disabled, this method is
        * expected to reset the setuid and setgid bits.
        */
        //int (*truncate) (const char *, off_t, struct fuse_file_info *fi);
        public IntPtr truncate;

        /** Open a file
        *
        * Open flags are available in fi->flags. The following rules
        * apply.
        *
        *  - Creation (O_CREAT, O_EXCL, O_NOCTTY) flags will be
        *    filtered out / handled by the kernel.
        *
        *  - Access modes (O_RDONLY, O_WRONLY, O_RDWR) should be used
        *    by the filesystem to check if the operation is
        *    permitted.  If the ``-o default_permissions`` mount
        *    option is given, this check is already done by the
        *    kernel before calling open() and may thus be omitted by
        *    the filesystem.
        *
        *  - When writeback caching is enabled, the kernel may send
        *    read requests even for files opened with O_WRONLY. The
        *    filesystem should be prepared to handle this.
        *
        *  - When writeback caching is disabled, the filesystem is
        *    expected to properly handle the O_APPEND flag and ensure
        *    that each write is appending to the end of the file.
        * 
            *  - When writeback caching is enabled, the kernel will
        *    handle O_APPEND. However, unless all changes to the file
        *    come through the kernel this will not work reliably. The
        *    filesystem should thus either ignore the O_APPEND flag
        *    (and let the kernel handle it), or return an error
        *    (indicating that reliably O_APPEND is not available).
        *
        * Filesystem may store an arbitrary file handle (pointer,
        * index, etc) in fi->fh, and use this in other all other file
        * operations (read, write, flush, release, fsync).
        *
        * Filesystem may also implement stateless file I/O and not store
        * anything in fi->fh.
        *
        * There are also some flags (direct_io, keep_cache) which the
        * filesystem may set in fi, to change the way the file is opened.
        * See fuse_file_info structure in <fuse_common.h> for more details.
        *
        * If this request is answered with an error code of ENOSYS
        * and FUSE_CAP_NO_OPEN_SUPPORT is set in
        * `fuse_conn_info.capable`, this is treated as success and
        * future calls to open will also succeed without being send
        * to the filesystem process.
        *
        */
        //int (*open) (const char *, struct fuse_file_info *);
        public IntPtr open;

        /** Read data from an open file
        *
        * Read should return exactly the number of bytes requested except
        * on EOF or error, otherwise the rest of the data will be
        * substituted with zeroes.	 An exception to this is when the
        * 'direct_io' mount option is specified, in which case the return
        * value of the read system call will reflect the return value of
        * this operation.
        */
        //int (*read) (const char *, char *, size_t, off_t,
        // struct fuse_file_info *);
        public IntPtr read;

        /** Write data to an open file
        *
        * Write should return exactly the number of bytes requested
        * except on error.	 An exception to this is when the 'direct_io'
        * mount option is specified (see read operation).
        *
        * Unless FUSE_CAP_HANDLE_KILLPRIV is disabled, this method is
        * expected to reset the setuid and setgid bits.
        */
        //int (*write) (const char *, const char *, size_t, off_t,
        // struct fuse_file_info *);
        public IntPtr write;

        /** Get file system statistics
        *
        * The 'f_favail', 'f_fsid' and 'f_flag' fields are ignored
        */
        //int (*statfs) (const char *, struct statvfs *);
        public IntPtr statfs;

        /** Possibly flush cached data
        *
        * BIG NOTE: This is not equivalent to fsync().  It's not a
        * request to sync dirty data.
        *
        * Flush is called on each close() of a file descriptor.  So if a
        * filesystem wants to return write errors in close() and the file
        * has cached dirty data, this is a good place to write back data
        * and return any errors.  Since many applications ignore close()
        * errors this is not always useful.
        *
        * NOTE: The flush() method may be called more than once for each
        * open().	This happens if more than one file descriptor refers
        * to an opened file due to dup(), dup2() or fork() calls.	It is
        * not possible to determine if a flush is final, so each flush
        * should be treated equally.  Multiple write-flush sequences are
        * relatively rare, so this shouldn't be a problem.
        *
        * Filesystems shouldn't assume that flush will always be called
        * after some writes, or that if will be called at all.
        */
        //int (*flush) (const char *, struct fuse_file_info *);
        public IntPtr flush;

        /** Release an open file
        *
        * Release is called when there are no more references to an open
        * file: all file descriptors are closed and all memory mappings
        * are unmapped.
        *
        * For every open() call there will be exactly one release() call
        * with the same flags and file descriptor.	 It is possible to
        * have a file opened more than once, in which case only the last
        * release will mean, that no more reads/writes will happen on the
        * file.  The return value of release is ignored.
        */
        //int (*release) (const char *, struct fuse_file_info *);
        public IntPtr release;

        /** Synchronize file contents
        *
        * If the datasync parameter is non-zero, then only the user data
        * should be flushed, not the meta data.
        */
        //int (*fsync) (const char *, int, struct fuse_file_info *);
        public IntPtr fsync;

        /** Set extended attributes */
        //int (*setxattr) (const char *, const char *, const char *, size_t, int);
        public IntPtr setxattr;

        /** Get extended attributes */
        //int (*getxattr) (const char *, const char *, char *, size_t);
        public IntPtr getxattr;

        /** List extended attributes */
        //int (*listxattr) (const char *, char *, size_t);
        public IntPtr listxattr;

        /** Remove extended attributes */
        //int (*removexattr) (const char *, const char *);
        public IntPtr removexattr;

        /** Open directory
        *
        * Unless the 'default_permissions' mount option is given,
        * this method should check if opendir is permitted for this
        * directory. Optionally opendir may also return an arbitrary
        * filehandle in the fuse_file_info structure, which will be
        * passed to readdir, closedir and fsyncdir.
        */
        //int (*opendir) (const char *, struct fuse_file_info *);
        public IntPtr opendir;

        /** Read directory
        *
        * The filesystem may choose between two modes of operation:
        *
        * 1) The readdir implementation ignores the offset parameter, and
        * passes zero to the filler function's offset.  The filler
        * function will not return '1' (unless an error happens), so the
        * whole directory is read in a single readdir operation.
        *
        * 2) The readdir implementation keeps track of the offsets of the
        * directory entries.  It uses the offset parameter and always
        * passes non-zero offset to the filler function.  When the buffer
        * is full (or an error happens) the filler function will return
        * '1'.
        */
        //int (*readdir) (const char *, void *, fuse_fill_dir_t, off_t,
        // struct fuse_file_info *, enum fuse_readdir_flags);
        public IntPtr readdir;

        /** Release directory
        */
        //int (*releasedir) (const char *, struct fuse_file_info *);
        public IntPtr releasedir;

        /** Synchronize directory contents
        *
        * If the datasync parameter is non-zero, then only the user data
        * should be flushed, not the meta data
        */
        //int (*fsyncdir) (const char *, int, struct fuse_file_info *);
        public IntPtr fsyncdir;

        /**
        * Initialize filesystem
        *
        * The return value will passed in the `private_data` field of
        * `struct fuse_context` to all file operations, and as a
        * parameter to the destroy() method. It overrides the initial
        * value provided to fuse_main() / fuse_new().
        */
        // void *(*init) (struct fuse_conn_info *conn,
        //         struct fuse_config *cfg);
        public IntPtr init;

        /**
        * Clean up filesystem
        *
        * Called on filesystem exit.
        */
        // void (*destroy) (void *private_data);
        IntPtr destroy;

        /**
        * Check file access permissions
        *
        * This will be called for the access() system call.  If the
        * 'default_permissions' mount option is given, this method is not
        * called.
        *
        * This method is not called under Linux kernel versions 2.4.x
        */
        //int (*access) (const char *, int);
        public IntPtr access;

        /**
        * Create and open a file
        *
        * If the file does not exist, first create it with the specified
        * mode, and then open it.
        *
        * If this method is not implemented or under Linux kernel
        * versions earlier than 2.6.15, the mknod() and open() methods
        * will be called instead.
        */
        //int (*create) (const char *, mode_t, struct fuse_file_info *);
        public IntPtr create;

        /**
        * Perform POSIX file locking operation
        *
        * The cmd argument will be either F_GETLK, F_SETLK or F_SETLKW.
        *
        * For the meaning of fields in 'struct flock' see the man page
        * for fcntl(2).  The l_whence field will always be set to
        * SEEK_SET.
        *
        * For checking lock ownership, the 'fuse_file_info->owner'
        * argument must be used.
        *
        * For F_GETLK operation, the library will first check currently
        * held locks, and if a conflicting lock is found it will return
        * information without calling this method.	 This ensures, that
        * for local locks the l_pid field is correctly filled in.	The
        * results may not be accurate in case of race conditions and in
        * the presence of hard links, but it's unlikely that an
        * application would rely on accurate GETLK results in these
        * cases.  If a conflicting lock is not found, this method will be
        * called, and the filesystem may fill out l_pid by a meaningful
        * value, or it may leave this field zero.
        *
        * For F_SETLK and F_SETLKW the l_pid field will be set to the pid
        * of the process performing the locking operation.
        *
        * Note: if this method is not implemented, the kernel will still
        * allow file locking to work locally.  Hence it is only
        * interesting for network filesystems and similar.
        */
        //int (*lock) (const char *, struct fuse_file_info *, int cmd,
        // struct flock *);
        IntPtr lock_;

        /**
        * Change the access and modification times of a file with
        * nanosecond resolution
        *
        * This supersedes the old utime() interface.  New applications
        * should use this.
        *
        * `fi` will always be NULL if the file is not currenly open, but
        * may also be NULL if the file is open.
        *
        * See the utimensat(2) man page for details.
        */
        //int (*utimens) (const char *, const struct timespec tv[2],
        // struct fuse_file_info *fi);
        public IntPtr utimens;

        /**
        * Map block index within file to block index within device
        *
        * Note: This makes sense only for block device backed filesystems
        * mounted with the 'blkdev' option
        */
        //int (*bmap) (const char *, size_t blocksize, uint64_t *idx);
        IntPtr bmap;

        /**
        * Ioctl
        *
        * flags will have FUSE_IOCTL_COMPAT set for 32bit ioctls in
        * 64bit environment.  The size and direction of data is
        * determined by _IOC_*() decoding of cmd.  For _IOC_NONE,
        * data will be NULL, for _IOC_WRITE data is out area, for
        * _IOC_READ in area and if both are set in/out area.  In all
        * non-NULL cases, the area is of _IOC_SIZE(cmd) bytes.
        *
        * If flags has FUSE_IOCTL_DIR then the fuse_file_info refers to a
        * directory file handle.
        */
        //int (*ioctl) (const char *, int cmd, void *arg,
        // struct fuse_file_info *, unsigned int flags, void *data);
        IntPtr ioctl;

        /**
        * Poll for IO readiness events
        *
        * Note: If ph is non-NULL, the client should notify
        * when IO readiness events occur by calling
        * fuse_notify_poll() with the specified ph.
        *
        * Regardless of the number of times poll with a non-NULL ph
        * is received, single notification is enough to clear all.
        * Notifying more times incurs overhead but doesn't harm
        * correctness.
        *
        * The callee is responsible for destroying ph with
        * fuse_pollhandle_destroy() when no longer in use.
        */
        //int (*poll) (const char *, struct fuse_file_info *,
        // struct fuse_pollhandle *ph, unsigned *reventsp);
        IntPtr poll;

        /** Write contents of buffer to an open file
        *
        * Similar to the write() method, but data is supplied in a
        * generic buffer.  Use fuse_buf_copy() to transfer data to
        * the destination.
        *
        * Unless FUSE_CAP_HANDLE_KILLPRIV is disabled, this method is
        * expected to reset the setuid and setgid bits.
        */
        //int (*write_buf) (const char *, struct fuse_bufvec *buf, off_t off,
        //                  struct fuse_file_info *);
        IntPtr write_buf;

        /** Store data from an open file in a buffer
        *
        * Similar to the read() method, but data is stored and
        * returned in a generic buffer.
        *
        * No actual copying of data has to take place, the source
        * file descriptor may simply be stored in the buffer for
        * later data transfer.
        *
        * The buffer must be allocated dynamically and stored at the
        * location pointed to by bufp.  If the buffer contains memory
        * regions, they too must be allocated using malloc().  The
        * allocated memory will be freed by the caller.
        */
        //int (*read_buf) (const char *, struct fuse_bufvec **bufp,
        // size_t size, off_t off, struct fuse_file_info *);
        IntPtr read_buf;
        /**
        * Perform BSD file locking operation
        *
        * The op argument will be either LOCK_SH, LOCK_EX or LOCK_UN
        *
        * Nonblocking requests will be indicated by ORing LOCK_NB to
        * the above operations
        *
        * For more information see the flock(2) manual page.
        *
        * Additionally fi->owner will be set to a value unique to
        * this open file.  This same value will be supplied to
        * ->release() when the file is released.
        *
        * Note: if this method is not implemented, the kernel will still
        * allow file locking to work locally.  Hence it is only
        * interesting for network filesystems and similar.
        */
        //int (*flock) (const char *, struct fuse_file_info *, int op);
        IntPtr flock;

        /**
        * Allocates space for an open file
        *
        * This function ensures that required space is allocated for specified
        * file.  If this function returns success then any subsequent write
        * request to specified range is guaranteed not to fail because of lack
        * of space on the file system media.
        */
        //int (*fallocate) (const char *, int, off_t, off_t,
        //                  struct fuse_file_info *);
        public IntPtr fallocate;
    };
}