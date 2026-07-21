using FuseSharp;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.MacFuse.AppModels;
using SecureFolderFS.Core.MacFuse.OpenHandles;

namespace SecureFolderFS.Core.MacFuse.Callbacks
{
    internal abstract class BaseMacFuseCallbacks : FuseFileSystemBase
    {
        protected FileSystemSpecifics specifics;
        protected readonly MacFuseHandlesManager handlesManager;

        protected BaseMacFuseCallbacks(FileSystemSpecifics specifics, MacFuseHandlesManager handlesManager)
        {
            this.specifics = specifics;
            this.handlesManager = handlesManager;
        }

        /// <remarks>
        /// Null before the filesystem has been mounted.
        /// </remarks>
        public MacFuseOptions? FuseOptions { get; set; }

        protected abstract string? GetCiphertextPath(ReadOnlySpan<byte> plaintextName);
    }
}
