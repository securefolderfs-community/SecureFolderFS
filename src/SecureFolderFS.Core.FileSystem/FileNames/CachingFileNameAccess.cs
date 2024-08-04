using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.FileSystem.FileNames
{
    /// <inheritdoc cref="FileNameAccess"/>
    internal sealed class CachingFileNameAccess : FileNameAccess
    {
        private readonly Dictionary<NameWithDirectoryId, string> _cleartextNames;
        private readonly Dictionary<NameWithDirectoryId, string> _ciphertextNames;

        public CachingFileNameAccess(Security security, IFileSystemStatistics fileSystemStatistics)
            : base(security, fileSystemStatistics)
        {
            _cleartextNames = new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_CLEARTEXT_FILENAMES);
            _ciphertextNames = new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES);
        }

        /// <inheritdoc/>
        public override string GetCleartextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            string cleartextName;
            string stringCiphertext = ciphertextName.ToString();

            lock (_cleartextNames)
            {
                if (!_cleartextNames.TryGetValue(new(directoryId.ToArray(), stringCiphertext), out cleartextName!))
                {
                    // Not found in cache
                    statistics.FileNameCache?.Report(CacheAccessType.CacheMiss);

                    // Get new cleartext name
                    var newCleartextName = base.GetCleartextName(ciphertextName, directoryId);
                    if (newCleartextName == string.Empty)
                        return string.Empty;

                    // Update cache
                    SetCleartextName(newCleartextName, stringCiphertext, directoryId);

                    return newCleartextName;
                }
            }

            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);
            statistics.FileNameCache?.Report(CacheAccessType.CacheHit);

            // Return existing cleartext name
            return cleartextName;
        }

        /// <inheritdoc/>
        public override string GetCiphertextName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId)
        {
            string ciphertextName;
            string stringCleartext = cleartextName.ToString();

            lock (_ciphertextNames)
            {
                if (!_ciphertextNames.TryGetValue(new(directoryId.ToArray(), stringCleartext), out ciphertextName!))
                {
                    // Not found in cache
                    statistics.FileNameCache?.Report(CacheAccessType.CacheMiss);

                    // Get new ciphertext name
                    var newCiphertextName = base.GetCiphertextName(cleartextName, directoryId);
                    if (newCiphertextName == string.Empty)
                        return string.Empty;

                    // Update cache
                    SetCiphertextName(newCiphertextName, stringCleartext, directoryId);

                    return newCiphertextName;
                }
            }

            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);
            statistics.FileNameCache?.Report(CacheAccessType.CacheHit);

            // Return existing ciphertext name
            return ciphertextName;
        }

        private void SetCleartextName(string cleartextName, string ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            if (_cleartextNames.Count >= FileSystem.Constants.Caching.RECOMMENDED_SIZE_CLEARTEXT_FILENAMES)
                _cleartextNames.Remove(_cleartextNames.Keys.First(), out _);

            _cleartextNames[new(directoryId.ToArray(), ciphertextName)] = cleartextName;
        }

        private void SetCiphertextName(string ciphertextName, string cleartextName, ReadOnlySpan<byte> directoryId)
        {
            if (_ciphertextNames.Count >= FileSystem.Constants.Caching.RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES)
                _ciphertextNames.Remove(_ciphertextNames.Keys.First(), out _);

            _ciphertextNames[new(directoryId.ToArray(), cleartextName)] = ciphertextName;
        }
    }
}
