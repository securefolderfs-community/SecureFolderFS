using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Core.Sdk.Tracking;

namespace SecureFolderFS.Core.Chunks.Receivers
{
    internal sealed class InstantAccessBasedChunkReceiver : BaseChunkReceiver
    {
        public InstantAccessBasedChunkReceiver(IChunkReaderDeprecated chunkReader, IChunkWriterDeprecated chunkWriter, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(chunkReader, chunkWriter, fileSystemStatsTracker)
        {
        }

        public override void Flush()
        {
        }
    }
}
