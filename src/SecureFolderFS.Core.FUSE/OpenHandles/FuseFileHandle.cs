using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    internal sealed class FuseFileHandle : FileHandle
    {
        public FuseFileHandle(Stream stream, FileAccess fileAccess, FileMode fileMode, string directory)
            : base(stream)
        {
            FileAccess = fileAccess;
            FileMode = fileMode;
            Directory = directory;
        }

        public FileAccess FileAccess { get; }
        public FileMode FileMode { get; }

        public string Directory { get; }
    }
}