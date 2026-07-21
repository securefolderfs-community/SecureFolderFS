using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.MacFuse.OpenHandles
{
    /// <inheritdoc cref="FileHandle"/>
    internal sealed class MacFuseFileHandle : FileHandle
    {
        public FileAccess FileAccess { get; }

        public FileMode FileMode { get; }

        public string Directory { get; }

        public MacFuseFileHandle(Stream stream, FileAccess fileAccess, FileMode fileMode, string directory)
            : base(stream)
        {
            FileAccess = fileAccess;
            FileMode = fileMode;
            Directory = directory;
        }
    }
}
