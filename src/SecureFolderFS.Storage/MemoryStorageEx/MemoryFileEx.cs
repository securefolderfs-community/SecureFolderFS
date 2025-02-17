using OwlCore.Storage.Memory;
using System.IO;

namespace SecureFolderFS.Storage.MemoryStorageEx
{
    /// <inheritdoc cref="MemoryFile"/>
    public class MemoryFileEx : MemoryFile
    {
        /// <inheritdoc/>
        public MemoryFileEx(MemoryStream memoryStream)
            : base(memoryStream)
        {
        }

        /// <inheritdoc/>
        public MemoryFileEx(string id, string name, MemoryStream memoryStream)
            : base(id, name, memoryStream)
        {
        }

        internal void SetParent(MemoryFolder memoryFolder)
        {
            Parent = memoryFolder;
        }
    }
}
