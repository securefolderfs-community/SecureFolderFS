using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Sdk.Tracking;

namespace SecureFolderFS.Core.Chunks.Receivers
{
    internal sealed class InstantAccessBasedChunkReceiver : BaseChunkReceiver
    {
        public InstantAccessBasedChunkReceiver(IChunkReader chunkReader, IChunkWriter chunkWriter, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(chunkReader, chunkWriter, fileSystemStatsTracker)
        {
        }

        public override void Flush()
        {
        }
    }
}
