using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Analytics;

namespace SecureFolderFS.Core.Models
{
    public sealed class FileSystemOptions
    {
        public FileSystemAdapterType FileSystemAdapterType { get; init; } // TODO: required modifier

        public ChunkCachingStrategy ChunkCachingStrategy { get; init; } = ChunkCachingStrategy.NoCache;

        public FileNameCachingStrategy FileNameCachingStrategy { get; init; } = FileNameCachingStrategy.RandomAccessMemoryCache;

        public DirectoryIdCachingStrategy DirectoryIdCachingStrategy { get; init; } = DirectoryIdCachingStrategy.RandomAccessMemoryCache;

        public IFileSystemStatsTracker? FileSystemStatsTracker { get; init; }
    }
}
