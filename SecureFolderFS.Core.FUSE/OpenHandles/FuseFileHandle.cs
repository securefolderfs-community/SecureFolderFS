using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    internal sealed class FuseFileHandle : FileHandle
    {
        public FuseFileHandle(Stream stream, FileAccess fileAccess, FileMode fileMode)
            : base(stream)
        {
            FileAccess = fileAccess;
            FileMode = fileMode;
        }

        public FileAccess FileAccess { get; }
        public FileMode FileMode { get; }
    }
}