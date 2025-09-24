using System.IO;
using OwlCore.Storage.Memory;

namespace SecureFolderFS.Storage.MemoryStorageEx
{
    /// <inheritdoc cref="MemoryFile"/>
    public class MemoryFileEx : MemoryFile
    {
        internal MemoryStream InternalStream { get; }

        /// <inheritdoc/>
        public MemoryFileEx(MemoryStream memoryStream, MemoryFolder? parent)
            : base(memoryStream)
        {
            Parent = parent;
            InternalStream = memoryStream;
        }

        /// <inheritdoc/>
        public MemoryFileEx(string id, string name, MemoryStream memoryStream, MemoryFolder? parent)
            : base(id, name, memoryStream)
        {
            Parent = parent;
            InternalStream = memoryStream;
        }

        internal void SetParent(MemoryFolder memoryFolder)
        {
            Parent = memoryFolder;
        }
    }
}
