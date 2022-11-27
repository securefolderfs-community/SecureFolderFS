using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.Analytics;

namespace SecureFolderFS.Core.Models
{
    public sealed class FileSystemOptions
    {
        public required FileSystemAdapterType FileSystemAdapterType { get; init; } // TODO: required modifier

        public IFileSystemStatsTracker? FileSystemStatsTracker { get; init; }

        public ChunkCachingStrategy ChunkCachingStrategy { get; init; } = ChunkCachingStrategy.RandomAccessMemoryCache;

        public FileNameCachingStrategy FileNameCachingStrategy { get; init; } = FileNameCachingStrategy.RandomAccessMemoryCache;

        public DirectoryIdCachingStrategy DirectoryIdCachingStrategy { get; init; } = DirectoryIdCachingStrategy.RandomAccessMemoryCache;
    }
}
