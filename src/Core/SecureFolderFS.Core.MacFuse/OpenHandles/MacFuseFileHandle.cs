using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.MacFuse.OpenHandles
{
    /// <inheritdoc cref="FileHandle"/>
    internal sealed class MacFuseFileHandle : FileHandle
    {
        public FileAccess FileAccess { get; }

        public FileMode FileMode { get; }

        public string Directory => Path.GetDirectoryName(CiphertextPath)!;

        /// <summary>
        /// Gets or sets the path of the ciphertext file this handle is open on. Kept current across renames.
        /// </summary>
        public string CiphertextPath { get; set; }

        public MacFuseFileHandle(Stream stream, FileAccess fileAccess, FileMode fileMode, string ciphertextPath)
            : base(stream)
        {
            FileAccess = fileAccess;
            FileMode = fileMode;
            CiphertextPath = ciphertextPath;
        }
    }
}
