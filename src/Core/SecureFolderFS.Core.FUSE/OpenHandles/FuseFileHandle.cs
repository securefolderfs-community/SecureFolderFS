using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FUSE.OpenHandles
{
    /// <inheritdoc cref="FileHandle"/>
    internal sealed class FuseFileHandle : FileHandle
    {
        public FileAccess FileAccess { get; }

        public FileMode FileMode { get; }

        public string Directory { get; }

        public FuseFileHandle(Stream stream, FileAccess fileAccess, FileMode fileMode, string directory)
            : base(stream)
        {
            FileAccess = fileAccess;
            FileMode = fileMode;
            Directory = directory;
        }
    }
}