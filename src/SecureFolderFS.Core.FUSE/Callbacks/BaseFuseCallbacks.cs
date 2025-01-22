using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.FUSE.OpenHandles;
using Tmds.Fuse;

namespace SecureFolderFS.Core.FUSE.Callbacks
{
    internal abstract class BaseFuseCallbacks : FuseFileSystemBase
    {
        protected readonly FuseHandlesManager handlesManager;

        public FileSystemSpecifics Specifics { get; }

        protected BaseFuseCallbacks(FileSystemSpecifics specifics, FuseHandlesManager handlesManager)
        {
            Specifics = specifics;
            this.handlesManager = handlesManager;
        }

        /// <remarks>
        /// Null before the filesystem has been mounted.
        /// </remarks>
        public FuseOptions? FuseOptions { get; set; } // TODO: Get instance from Specifics.FileSystemOptions and cast to FuseOptions

        protected abstract string? GetCiphertextPath(ReadOnlySpan<byte> plaintextName);
    }
}