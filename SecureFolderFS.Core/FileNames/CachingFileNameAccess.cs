using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.Sdk.Tracking;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SecureFolderFS.Core.FileNames
{
    /// <inheritdoc cref="IFileNameAccess"/>
    internal sealed class CachingFileNameAccess : FileNameAccess
    {
        private readonly ConcurrentDictionary<NameWithDirectoryId, ReadOnlyMemory<char>> _cleartextNames;
        private readonly ConcurrentDictionary<NameWithDirectoryId, ReadOnlyMemory<char>> _ciphertextNames;

        public CachingFileNameAccess(ISecurity security, IFileSystemStatsTracker? statsTracker)
            : base(security, statsTracker)
        {
            _cleartextNames = new(3, Constants.IO.MAX_CACHED_CLEARTEXT_FILENAMES);
            _ciphertextNames = new(3, Constants.IO.MAX_CACHED_CIPHERTEXT_FILENAMES);
        }

        /// <inheritdoc/>
        public override ReadOnlySpan<char> GetCleartextName(ReadOnlySpan<char> ciphertextName, DirectoryId directoryId)
        {
            if (!_cleartextNames.TryGetValue(new(directoryId, ciphertextName.ToArray()), out var cleartextName))
            {
                // Not found in cache
                statsTracker?.AddFileNameCacheMiss();

                // Get new cleartext name
                var newCleartextName = base.GetCleartextName(ciphertextName, directoryId);
                if (newCleartextName.IsEmpty)
                    return ReadOnlySpan<char>.Empty;

                // Update cache
                SetCleartextName(cleartextName, ciphertextName.ToArray(), directoryId);

                return newCleartextName;
            }

            statsTracker?.AddFileNameAccess();
            statsTracker?.AddFileNameCacheHit();

            // Return existing cleartext name
            return cleartextName.ToArray();
        }

        /// <inheritdoc/>
        public override ReadOnlySpan<char> GetCiphertextName(ReadOnlySpan<char> cleartextName, DirectoryId directoryId)
        {
            if (!_ciphertextNames.TryGetValue(new(directoryId, cleartextName.ToArray()), out var ciphertextName))
            {
                // Not found in cache
                statsTracker?.AddFileNameCacheMiss();

                // Get new ciphertext name
                var newCiphertextName = base.GetCiphertextName(cleartextName, directoryId);
                if (newCiphertextName.IsEmpty)
                    return ReadOnlySpan<char>.Empty;

                // Update cache
                SetCiphertextName(ciphertextName, cleartextName.ToArray(), directoryId);

                return newCiphertextName;
            }

            statsTracker?.AddFileNameAccess();
            statsTracker?.AddFileNameCacheHit();

            // Return existing ciphertext name
            return ciphertextName.ToArray();
        }

        private void SetCleartextName(ReadOnlyMemory<char> cleartextName, ReadOnlyMemory<char> ciphertextName, DirectoryId directoryId)
        {
            if (_cleartextNames.Count >= Constants.IO.MAX_CACHED_CLEARTEXT_FILENAMES)
                _cleartextNames.TryRemove(_cleartextNames.Keys.First(), out _);

            _cleartextNames[new(directoryId, ciphertextName)] = cleartextName;
        }

        private void SetCiphertextName(ReadOnlyMemory<char> ciphertextName, ReadOnlyMemory<char> cleartextName, DirectoryId directoryId)
        {
            if (_ciphertextNames.Count >= Constants.IO.MAX_CACHED_CIPHERTEXT_FILENAMES)
                _ciphertextNames.TryRemove(_ciphertextNames.Keys.First(), out _);

            _ciphertextNames[new(directoryId, cleartextName)] = ciphertextName;
        }

        private record NameWithDirectoryId(DirectoryId DirectoryId, ReadOnlyMemory<char> FileName);
    }
}
