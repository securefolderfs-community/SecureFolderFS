using System;
using System.Linq;
using System.Collections.Generic;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Tracking;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Core.FileNames.Receivers
{
    internal sealed class RandomAccessMemoryBasedFileNameReceiver : BaseFileNameReceiver
    {
        private readonly Dictionary<FileNameWithDirectoryId, string> _ciphertextFileNames;

        private readonly Dictionary<FileNameWithDirectoryId, string> _cleartextFileNames;

        public RandomAccessMemoryBasedFileNameReceiver(ISecurity security, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(security, fileSystemStatsTracker)
        {
            this._ciphertextFileNames = new Dictionary<FileNameWithDirectoryId, string>();
            this._cleartextFileNames = new Dictionary<FileNameWithDirectoryId, string>();
        }

        public override string GetCleartextFileName(DirectoryId directoryId, string ciphertextFileName)
        {
            AssertNotDisposed();

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
            AssertNotDisposed();

            if (!_ciphertextFileNames.TryGetValue(new FileNameWithDirectoryId(directoryId, cleartextFileName), out string ciphertextFileName))
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
            AssertNotDisposed();

            if (_cleartextFileNames.Count >= Constants.IO.MAX_CACHED_CLEARTEXT_FILENAMES)
            {
                var fileNameWithDirectoryIdToRemove = _cleartextFileNames.Keys.First();
                _cleartextFileNames.Remove(fileNameWithDirectoryIdToRemove);
            }

            _cleartextFileNames.AddOrReplace(new(directoryId, ciphertextFileName), cleartextFileName);
        }

        public override void SetCiphertextFileName(DirectoryId directoryId, string cleartextFileName, string ciphertextFileName)
        {
            AssertNotDisposed();

            if (_ciphertextFileNames.Count >= Constants.IO.MAX_CACHED_CIPHERTEXT_FILENAMES)
            {
                var fileNameWithDirectoryIdToRemove = _ciphertextFileNames.Keys.First();
                _ciphertextFileNames.Remove(fileNameWithDirectoryIdToRemove);
            }

            _ciphertextFileNames.AddOrReplace(new(directoryId, cleartextFileName), ciphertextFileName);
        }

        public override void Dispose()
        {
            base.Dispose();
            _ciphertextFileNames.Clear();
            _cleartextFileNames.Clear();
        }
    }
}
