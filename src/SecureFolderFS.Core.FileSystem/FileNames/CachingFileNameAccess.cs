using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.FileSystem.FileNames
{
    /// <inheritdoc cref="FileNameAccess"/>
    internal sealed class CachingFileNameAccess : FileNameAccess
    {
        private readonly Dictionary<NameWithDirectoryId, string> _PlaintextNames;
        private readonly Dictionary<NameWithDirectoryId, string> _ciphertextNames;

        public CachingFileNameAccess(Security security, IFileSystemStatistics fileSystemStatistics)
            : base(security, fileSystemStatistics)
        {
            _PlaintextNames = new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_Plaintext_FILENAMES);
            _ciphertextNames = new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES);
        }

        /// <inheritdoc/>
        public override string GetPlaintextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            string PlaintextName;
            string stringCiphertext = ciphertextName.ToString();

            lock (_PlaintextNames)
            {
                if (!_PlaintextNames.TryGetValue(new(directoryId.ToArray(), stringCiphertext), out PlaintextName!))
                {
                    // Not found in cache
                    statistics.FileNameCache?.Report(CacheAccessType.CacheMiss);

                    // Get new plaintext name
                    var newPlaintextName = base.GetPlaintextName(ciphertextName, directoryId);
                    if (newPlaintextName == string.Empty)
                        return string.Empty;

                    // Update cache
                    SetPlaintextName(newPlaintextName, stringCiphertext, directoryId);

                    return newPlaintextName;
                }
            }

            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);
            statistics.FileNameCache?.Report(CacheAccessType.CacheHit);

            // Return existing plaintext name
            return PlaintextName;
        }

        /// <inheritdoc/>
        public override string GetCiphertextName(ReadOnlySpan<char> PlaintextName, ReadOnlySpan<byte> directoryId)
        {
            string ciphertextName;
            string stringPlaintext = PlaintextName.ToString();

            lock (_ciphertextNames)
            {
                if (!_ciphertextNames.TryGetValue(new(directoryId.ToArray(), stringPlaintext), out ciphertextName!))
                {
                    // Not found in cache
                    statistics.FileNameCache?.Report(CacheAccessType.CacheMiss);

                    // Get new ciphertext name
                    var newCiphertextName = base.GetCiphertextName(PlaintextName, directoryId);
                    if (newCiphertextName == string.Empty)
                        return string.Empty;

                    // Update cache
                    SetCiphertextName(newCiphertextName, stringPlaintext, directoryId);

                    return newCiphertextName;
                }
            }

            statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);
            statistics.FileNameCache?.Report(CacheAccessType.CacheHit);

            // Return existing ciphertext name
            return ciphertextName;
        }

        private void SetPlaintextName(string PlaintextName, string ciphertextName, ReadOnlySpan<byte> directoryId)
        {
            if (_PlaintextNames.Count >= FileSystem.Constants.Caching.RECOMMENDED_SIZE_Plaintext_FILENAMES)
                _PlaintextNames.Remove(_PlaintextNames.Keys.First(), out _);

            _PlaintextNames[new(directoryId.ToArray(), ciphertextName)] = PlaintextName;
        }

        private void SetCiphertextName(string ciphertextName, string PlaintextName, ReadOnlySpan<byte> directoryId)
        {
            if (_ciphertextNames.Count >= FileSystem.Constants.Caching.RECOMMENDED_SIZE_CIPHERTEXT_FILENAMES)
                _ciphertextNames.Remove(_ciphertextNames.Keys.First(), out _);

            _ciphertextNames[new(directoryId.ToArray(), PlaintextName)] = ciphertextName;
        }
    }
}
