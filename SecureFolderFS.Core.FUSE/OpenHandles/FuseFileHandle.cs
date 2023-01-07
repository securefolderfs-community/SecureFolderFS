using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    internal sealed class FuseFileHandle : FileHandle
    {
        public FuseFileHandle(Stream stream)
            : base(stream)
        {
        }
    }
}