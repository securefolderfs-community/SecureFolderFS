using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.Sdk.Tracking;
using System.Collections.Concurrent;
using System.Linq;

namespace SecureFolderFS.Core.FileNames.Receivers
{
    internal sealed class RandomAccessMemoryBasedFileNameReceiver : BaseFileNameReceiver
    {
        private readonly ConcurrentDictionary<FileNameWithDirectoryId, string> _ciphertextFileNames;
        private readonly ConcurrentDictionary<FileNameWithDirectoryId, string> _cleartextFileNames;

        public RandomAccessMemoryBasedFileNameReceiver(ISecurity security, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(security, fileSystemStatsTracker)
        {
            _ciphertextFileNames = new(3, Constants.IO.MAX_CACHED_CIPHERTEXT_FILENAMES);
            _cleartextFileNames = new(3, Constants.IO.MAX_CACHED_CLEARTEXT_FILENAMES);
        }

        public override string GetCleartextFileName(DirectoryId directoryId, string ciphertextFileName)
        {
            if (!_cleartextFileNames.TryGetValue(new FileNameWithDirectoryId(directoryId, ciphertextFileName), out string cleartextFileName))
            {
                fileSystemStatsTracker?.AddFileNameCacheMiss();
                cleartextFileName = base.GetCleartextFileName(directoryId, ciphertextFileName);
                SetCleartextFileName(directoryId, ciphertextFileName, cleartextFileName);
            }
            else
            {
                fileSystemStatsTracker?.AddFileNameAccess();
                fileSystemStatsTracker?.AddFileNameCacheHit();
            }

            return cleartextFileName;
        }

        public override string GetCiphertextFileName(DirectoryId directoryId, string cleartextFileName)
        {
            if (!_ciphertextFileNames.TryGetValue(new FileNameWithDirectoryId(directoryId, cleartextFileName), out var ciphertextFileName))
            {
                fileSystemStatsTracker?.AddFileNameCacheMiss();
                ciphertextFileName = base.GetCiphertextFileName(directoryId, cleartextFileName);
                SetCiphertextFileName(directoryId, cleartextFileName, ciphertextFileName);
            }
            else
            {
                fileSystemStatsTracker?.AddFileNameAccess();
                fileSystemStatsTracker?.AddFileNameCacheHit();
            }

            return ciphertextFileName;
        }

        public override void SetCleartextFileName(DirectoryId directoryId, string ciphertextFileName, string cleartextFileName)
        {
            if (_cleartextFileNames.Count >= Constants.IO.MAX_CACHED_CLEARTEXT_FILENAMES)
            {
                var fileNameWithDirectoryIdToRemove = _cleartextFileNames.Keys.First();
                _cleartextFileNames.TryRemove(fileNameWithDirectoryIdToRemove, out _);
            }

            _cleartextFileNames[new(directoryId, ciphertextFileName)] = cleartextFileName;
        }

        public override void SetCiphertextFileName(DirectoryId directoryId, string cleartextFileName, string ciphertextFileName)
        {
            if (_ciphertextFileNames.Count >= Constants.IO.MAX_CACHED_CIPHERTEXT_FILENAMES)
            {
                var fileNameWithDirectoryIdToRemove = _ciphertextFileNames.Keys.First();
                _ciphertextFileNames.TryRemove(fileNameWithDirectoryIdToRemove, out _);
            }

            _ciphertextFileNames[new(directoryId, cleartextFileName)] = ciphertextFileName;
        }
    }
}
