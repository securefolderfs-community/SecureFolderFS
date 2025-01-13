﻿using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class AndroidVFSRoot : VFSRoot
    {
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.Android.FileSystem.FS_NAME;

        public AndroidVFSRoot(IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
        {
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                FileSystemManager.Instance.RemoveRoot(this);
            }

            return ValueTask.CompletedTask;
        }
    }
}
