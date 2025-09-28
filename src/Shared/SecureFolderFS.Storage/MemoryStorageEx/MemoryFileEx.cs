using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage.Memory;
using SecureFolderFS.Storage.Streams;

namespace SecureFolderFS.Storage.MemoryStorageEx
{
    /// <inheritdoc cref="MemoryFile"/>
    public class MemoryFileEx : MemoryFile
    {
        private readonly IStreamSource? _streamSource;

        internal MemoryStream InternalStream { get; }

        public MemoryFileEx(MemoryStream memoryStream, MemoryFolder? parent, IStreamSource? streamSource = null)
            : base(memoryStream)
        {
            _streamSource = streamSource;
            Parent = parent;
            InternalStream = memoryStream;
        }

        public MemoryFileEx(string id, string name, MemoryStream memoryStream, MemoryFolder? parent, IStreamSource? streamSource = null)
            : base(id, name, memoryStream)
        {
            _streamSource = streamSource;
            Parent = parent;
            InternalStream = memoryStream;
        }

        /// <inheritdoc/>
        public override async Task<Stream> OpenStreamAsync(FileAccess accessMode = FileAccess.Read, CancellationToken cancellationToken = new CancellationToken())
        {
            return _streamSource?.WrapStreamSource(accessMode, InternalStream) ?? await base.OpenStreamAsync(accessMode, cancellationToken);
        }

        internal void SetParent(MemoryFolder memoryFolder)
        {
            Parent = memoryFolder;
        }
    }
}
