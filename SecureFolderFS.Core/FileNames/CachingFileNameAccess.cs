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
        private readonly Dictionary<NameWithDirectoryId, string> _cleartextNames;
        private readonly Dictionary<NameWithDirectoryId, string> _ciphertextNames;

        public CachingFileNameAccess(Security security, IFileSystemStatistics? fileSystemStatistics)
            : base(security, fileSystemStatistics)
        {
            _cleartextNames = new(Constants.Caching.CLEARTEXT_FILENAMES_CACHE_SIZE);
            _ciphertextNames = new(Constants.Caching.CIPHERTEXT_FILENAMES_CACHE_SIZE);
        }

        /// <inheritdoc/>
        public override string GetCleartextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            lock (_cleartextNames)
            {
                var stringCiphertext = ciphertextName.ToString();
                if (!_cleartextNames.TryGetValue(new(directoryId.ToArray(), stringCiphertext), out var cleartextName))
                {
                    // Not found in cache
                    fileSystemStatistics?.NotifyFileNameCacheMiss();

                    // Get new cleartext name
                    var newCleartextName = base.GetCleartextName(ciphertextName, directoryId);
                    if (newCleartextName == string.Empty)
                        return string.Empty;

                    // Update cache
                    SetCleartextName(newCleartextName, stringCiphertext, directoryId);

                    return newCleartextName;
                }

                fileSystemStatistics?.NotifyFileNameAccess();
                fileSystemStatistics?.NotifyFileNameCacheHit();

                // Return existing cleartext name
                return cleartextName.ToString();
            }
        }

        /// <inheritdoc/>
        public override string GetCiphertextName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId)
        {
            lock (_ciphertextNames)
            {
                var stringCleartext = cleartextName.ToString();
                if (!_ciphertextNames.TryGetValue(new(directoryId.ToArray(), stringCleartext), out var ciphertextName))
                {
                    // Not found in cache
                    fileSystemStatistics?.NotifyFileNameCacheMiss();

                    // Get new ciphertext name
                    var newCiphertextName = base.GetCiphertextName(cleartextName, directoryId);
                    if (newCiphertextName == string.Empty)
                        return string.Empty;

                    // Update cache
                    SetCiphertextName(newCiphertextName, stringCleartext, directoryId);

                    return newCiphertextName;
                }

                fileSystemStatistics?.NotifyFileNameAccess();
                fileSystemStatistics?.NotifyFileNameCacheHit();

                // Return existing ciphertext name
                return ciphertextName;
            }
        }

        private void SetCleartextName(string cleartextName, string ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            if (_cleartextNames.Count >= Constants.Caching.CLEARTEXT_FILENAMES_CACHE_SIZE)
                _cleartextNames.Remove(_cleartextNames.Keys.First(), out _);

            _cleartextNames[new(directoryId.ToArray(), ciphertextName)] = cleartextName;
        }

        private void SetCiphertextName(string ciphertextName, string cleartextName, ReadOnlySpan<byte> directoryId)
        {
            if (_ciphertextNames.Count >= Constants.Caching.CIPHERTEXT_FILENAMES_CACHE_SIZE)
                _ciphertextNames.Remove(_ciphertextNames.Keys.First(), out _);

            _ciphertextNames[new(directoryId.ToArray(), cleartextName)] = ciphertextName;
        }

        // TODO: Replace with something else
        private sealed record NameWithDirectoryId(byte[] DirectoryId, string FileName);
    }
}
