using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Provides in-memory caching for thumbnails using a combined key of file path hash and modification date.
    /// When a file is modified, the old cached thumbnail is automatically invalidated.
    /// </summary>
    public sealed class ThumbnailCacheModel : IDisposable
    {
        /// <summary>
        /// Gets the default maximum number of cached thumbnails.
        /// </summary>
        public const int DEFAULT_MAX_ENTRIES = 100;

        private readonly IDatabaseModel<string> _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailCacheModel"/> class with default capacity.
        /// </summary>
        public ThumbnailCacheModel()
            : this(DEFAULT_MAX_ENTRIES)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailCacheModel"/> class.
        /// </summary>
        /// <param name="maxEntries">The maximum number of thumbnails to cache. Oldest entries are evicted when exceeded.</param>
        public ThumbnailCacheModel(int maxEntries)
        {
            _database = new InMemoryDatabaseModel<byte[]>(maxEntries);
        }

        /// <summary>
        /// Tries to get a cached thumbnail for the specified file.
        /// The cache key includes the file's modification date, so modified files automatically get new thumbnails.
        /// </summary>
        /// <param name="file">The file to get the cached thumbnail for.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the cached thumbnail stream if found, otherwise null.</returns>
        public async Task<Stream?> TryGetCachedThumbnailAsync(IFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = await GetCacheKeyAsync(file, cancellationToken);
                var cachedData = await _database.GetValueAsync<byte[]>(cacheKey, cancellationToken: cancellationToken);

                if (cachedData is null || cachedData.Length == 0)
                    return null;

                return new MemoryStream(cachedData);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Caches the thumbnail for the specified file.
        /// The cache key includes the file's modification date, ensuring modified files get fresh thumbnails.
        /// </summary>
        /// <param name="file">The file to cache the thumbnail for.</param>
        /// <param name="thumbnailStream">The thumbnail stream to cache.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task CacheThumbnailAsync(IFile file, IImageStream thumbnailStream, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = await GetCacheKeyAsync(file, cancellationToken);

                // Copy thumbnail to byte array
                using var memoryStream = new MemoryStream();
                await thumbnailStream.CopyToAsync(memoryStream, cancellationToken);
                var data = memoryStream.ToArray();

                await _database.SetValueAsync(cacheKey, data, cancellationToken);
            }
            catch (Exception)
            {
                // Silently fail - caching is optional
            }
        }

        /// <summary>
        /// Clears all cached thumbnails.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            return _database.WipeAsync(cancellationToken);
        }

        /// <summary>
        /// Generates a cache key combining the file path hash and modification date.
        /// This ensures that when a file is modified, the old cache entry becomes orphaned
        /// and a new entry with the updated date is created.
        /// </summary>
        /// <param name="file">The file to generate a cache key for.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A unique cache key string.</returns>
        private static async Task<string> GetCacheKeyAsync(IFile file, CancellationToken cancellationToken)
        {
            var dateModified = await file.GetDateModifiedAsync(cancellationToken);
            var pathHash = GetPathHash(file.Id);

            // Combine path hash with modification date for the cache key
            // Format: {pathHash}_{dateModifiedTicks}
            return $"{pathHash}_{dateModified.Ticks}";
        }

        /// <summary>
        /// Generates a hash of the file path using SHA256.
        /// </summary>
        /// <param name="filePath">The file path to hash.</param>
        /// <returns>A hexadecimal hash string.</returns>
        private static string GetPathHash(string filePath)
        {
            var bytes = Encoding.UTF8.GetBytes(filePath);
            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
