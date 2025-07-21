using OwlCore.Storage.Memory;
using System.IO;

namespace SecureFolderFS.Storage.MemoryStorageEx
{
    /// <inheritdoc cref="MemoryFile"/>
    public class MemoryFileEx : MemoryFile
    {
        internal MemoryStream InternalStream { get; }

        /// <inheritdoc/>
        public MemoryFileEx(MemoryStream memoryStream)
            : base(memoryStream)
        {
            InternalStream = memoryStream;
        }

        /// <inheritdoc/>
        public MemoryFileEx(string id, string name, MemoryStream memoryStream)
            : base(id, name, memoryStream)
        {
            InternalStream = memoryStream;
        }

        internal void SetParent(MemoryFolder memoryFolder)
        {
            Parent = memoryFolder;
        }
    }
}
