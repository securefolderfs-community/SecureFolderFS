using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.FileNames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.FileNames
{
    /// <inheritdoc cref="IFileNameAccess"/>
    internal sealed class CachingFileNameAccess : InstantFileNameAccess
    {
        private readonly Dictionary<NameWithDirectoryId, ReadOnlyMemory<char>> _cleartextNames;
        private readonly Dictionary<NameWithDirectoryId, ReadOnlyMemory<char>> _ciphertextNames;

        public CachingFileNameAccess(Security security, IFileSystemStatistics? fileSystemStatistics)
            : base(security, fileSystemStatistics)
        {
            _cleartextNames = new(Constants.Caching.CLEARTEXT_FILENAMES_CACHE_SIZE);
            _ciphertextNames = new(Constants.Caching.CIPHERTEXT_FILENAMES_CACHE_SIZE);
        }

        /// <inheritdoc/>
        public override ReadOnlySpan<char> GetCleartextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            lock (_cleartextNames)
            {
                if (!_cleartextNames.TryGetValue(new(directoryId.ToArray(), ciphertextName.ToArray()), out var cleartextName))
                {
                    // Not found in cache
                    fileSystemStatistics?.NotifyFileNameCacheMiss();

                    // Get new cleartext name
                    var newCleartextName = base.GetCleartextName(ciphertextName, directoryId);
                    if (newCleartextName.IsEmpty)
                        return ReadOnlySpan<char>.Empty;

                    // Update cache
                    SetCleartextName(cleartextName, ciphertextName.ToArray(), directoryId);

                    return newCleartextName;
                }

                fileSystemStatistics?.NotifyFileNameAccess();
                fileSystemStatistics?.NotifyFileNameCacheHit();

                // Return existing cleartext name
                return cleartextName.ToArray();
            }
        }

        /// <inheritdoc/>
        public override ReadOnlySpan<char> GetCiphertextName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId)
        {
            lock (_ciphertextNames)
            {
                if (!_ciphertextNames.TryGetValue(new(directoryId.ToArray(), cleartextName.ToArray()), out var ciphertextName))
                {
                    // Not found in cache
                    fileSystemStatistics?.NotifyFileNameCacheMiss();

                    // Get new ciphertext name
                    var newCiphertextName = base.GetCiphertextName(cleartextName, directoryId);
                    if (newCiphertextName.IsEmpty)
                        return ReadOnlySpan<char>.Empty;

                    // Update cache
                    SetCiphertextName(ciphertextName, cleartextName.ToArray(), directoryId);

                    return newCiphertextName;
                }

                fileSystemStatistics?.NotifyFileNameAccess();
                fileSystemStatistics?.NotifyFileNameCacheHit();

                // Return existing ciphertext name
                return ciphertextName.ToArray();
            }
        }

        private void SetCleartextName(ReadOnlyMemory<char> cleartextName, ReadOnlyMemory<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            if (_cleartextNames.Count >= Constants.Caching.CLEARTEXT_FILENAMES_CACHE_SIZE)
                _cleartextNames.Remove(_cleartextNames.Keys.First(), out _);

            _cleartextNames[new(directoryId.ToArray(), ciphertextName)] = cleartextName;
        }

        private void SetCiphertextName(ReadOnlyMemory<char> ciphertextName, ReadOnlyMemory<char> cleartextName, ReadOnlySpan<byte> directoryId)
        {
            if (_ciphertextNames.Count >= Constants.Caching.CIPHERTEXT_FILENAMES_CACHE_SIZE)
                _ciphertextNames.Remove(_ciphertextNames.Keys.First(), out _);

            _ciphertextNames[new(directoryId.ToArray(), cleartextName)] = ciphertextName;
        }

        // TODO: Replace with something else. Perhaps GetHashCode()?
        private sealed record NameWithDirectoryId(ReadOnlyMemory<byte> DirectoryId, ReadOnlyMemory<char> FileName);
    }
}
