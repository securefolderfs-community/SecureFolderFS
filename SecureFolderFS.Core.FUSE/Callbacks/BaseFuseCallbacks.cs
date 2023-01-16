using SecureFolderFS.Core.FileSystem.Paths;
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

        protected abstract string? GetCiphertextPath(ReadOnlySpan<byte> cleartextName);
    }
}