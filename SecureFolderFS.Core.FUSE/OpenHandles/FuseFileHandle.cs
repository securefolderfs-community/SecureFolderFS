using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    internal sealed class FuseFileHandle : FileHandle
    {
        public FuseFileHandle(Stream stream, FileAccess fileAccess)
            : base(stream)
        {
            FileAccess = fileAccess;
        }

        public FileAccess FileAccess { get; }
    }
}