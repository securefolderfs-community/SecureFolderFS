﻿using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class DokanyVFSRoot : VFSRoot
    {
        private readonly DokanyWrapper _dokanyWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public DokanyVFSRoot(DokanyWrapper dokanyWrapper, IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
        {
            _dokanyWrapper = dokanyWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await Task.Run(_dokanyWrapper.CloseFileSystem);
            if (_disposed)
            {
                FileSystemManager.Instance.FileSystems.Remove(this);
                await base.DisposeAsync();
            }
        }
    }
}
