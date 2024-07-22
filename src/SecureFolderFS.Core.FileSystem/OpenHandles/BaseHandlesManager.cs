using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SecureFolderFS.Core.FileSystem.OpenHandles
{
    /// <summary>
    /// Manager for persisting file and directory handles on the virtual file system.
    /// </summary>
    public abstract class BaseHandlesManager : IDisposable
    {
        protected bool disposed;
        protected readonly object handlesLock = new();
        protected readonly StreamsAccess streamsAccess;
        protected readonly HandlesGenerator handlesGenerator;
        protected readonly Dictionary<ulong, IDisposable> handles;

        protected BaseHandlesManager(StreamsAccess streamsAccess)
        {
            this.streamsAccess = streamsAccess;
            this.handlesGenerator = new();
            this.handles = new();
        }

        /// <summary>
        /// Opens a new handle for <paramref name="ciphertextPath"/> and returns its handle ID.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path that points to a file.</param>
        /// <param name="mode">The <see cref="FileMode"/> to open the file with.</param>
        /// <param name="access">The <see cref="FileAccess"/> to open the file with.</param>
        /// <param name="share">The <see cref="FileShare"/> to open the file with.</param>
        /// <param name="options">The <see cref="FileOptions"/> to open the file with.</param>
        /// <returns>If successful, value is non-zero.</returns>
        public abstract ulong OpenFileHandle(string ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options);

        /// <summary>
        /// Opens a new handle for <paramref name="ciphertextPath"/> and returns its handle ID.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path that points to a directory.</param>
        /// <returns>If successful, value is non-zero.</returns>
        public abstract ulong OpenDirectoryHandle(string ciphertextPath); // TODO: Add open flags as needed

        /// <summary>
        /// Gets a handle of type <typeparamref name="THandle"/> with associated <paramref name="handleId"/>.
        /// </summary>
        /// <typeparam name="THandle">The type of handle.</typeparam>
        /// <param name="handleId">The ID of the handle.</param>
        /// <returns>If successful, returns an instance of <typeparamref name="THandle"/>; otherwise false.</returns>
        public virtual THandle? GetHandle<THandle>(ulong handleId)
            where THandle : class, IDisposable
        {
            if (disposed || handleId == Constants.INVALID_HANDLE)
                return null;

            lock (handlesLock)
            {
                if (!handles.TryGetValue(handleId, out var handleObject))
                    return null;

                return (THandle?)handleObject;
            }
        }

        /// <summary>
        /// Removes <paramref name="handleId"/> from the manager and disposes of the <see cref="ObjectHandle"/>.
        /// </summary>
        /// <param name="handleId">The ID of the handle.</param>
        public virtual void CloseHandle(ulong handleId)
        {
            if (disposed || handleId == Constants.INVALID_HANDLE)
                return;

            lock (handlesLock)
            {
                if (handles.Remove(handleId, out var handleObject))
                    handleObject.Dispose();
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            lock (handlesLock)
            {
                handles.Values.DisposeElements();
                handles.Clear();
            }
        }

        protected sealed class HandlesGenerator
        {
            private ulong _handleCounter;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ulong ThreadSafeIncrement()
            {
                Interlocked.Increment(ref _handleCounter);
                return _handleCounter;
            }
        }
    }
}
