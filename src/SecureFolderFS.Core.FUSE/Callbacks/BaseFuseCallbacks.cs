using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.FUSE.OpenHandles;
using Tmds.Fuse;

namespace SecureFolderFS.Core.FUSE.Callbacks
{
    internal abstract class BaseFuseCallbacks : FuseFileSystemBase
    {
        protected readonly IPathConverter pathConverter;
        protected readonly FuseHandlesManager handlesManager;

        protected BaseFuseCallbacks(IPathConverter pathConverter, FuseHandlesManager handlesManager)
        {
            this.pathConverter = pathConverter;
            this.handlesManager = handlesManager;
        }

        /// <remarks>
        /// Null before the filesystem has been mounted.
        /// </remarks>
        public FuseMountOptions? MountOptions { get; set; }

        protected abstract string? GetCiphertextPath(ReadOnlySpan<byte> cleartextName);
    }
}