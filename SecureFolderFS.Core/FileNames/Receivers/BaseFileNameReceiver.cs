using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.Security;

namespace SecureFolderFS.Core.FileNames.Receivers
{
    internal abstract class BaseFileNameReceiver : IFileNameReceiver
    {
        protected readonly ISecurity security;
        protected readonly IFileSystemStatsTracker? fileSystemStatsTracker;

        protected BaseFileNameReceiver(ISecurity security, IFileSystemStatsTracker? fileSystemStatsTracker)
        {
            this.security = security;
            this.fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public virtual string GetCleartextFileName(DirectoryId directoryId, string ciphertextFileName)
        {
            fileSystemStatsTracker?.AddFileNameAccess();

            var cleartextFileName = security.FileNameCryptor.DecryptFileName(
                PathHelpers.RemoveExtension(ciphertextFileName, Constants.ENCRYPTED_FILE_EXTENSION), directoryId);

            SetCleartextFileName(directoryId, ciphertextFileName, cleartextFileName);
            SetCiphertextFileName(directoryId, cleartextFileName, ciphertextFileName);

            return cleartextFileName;
        }

        public virtual string GetCiphertextFileName(DirectoryId directoryId, string cleartextFileName)
        {
            fileSystemStatsTracker?.AddFileNameAccess();

            var ciphertextFileName = PathHelpers.AppendExtension(security.FileNameCryptor.EncryptFileName(cleartextFileName, directoryId),
                    Constants.ENCRYPTED_FILE_EXTENSION);

            SetCiphertextFileName(directoryId, cleartextFileName, ciphertextFileName);
            SetCleartextFileName(directoryId, ciphertextFileName, cleartextFileName);

            return ciphertextFileName;
        }

        public abstract void SetCleartextFileName(DirectoryId directoryId, string ciphertextFileName, string cleartextFileName);

        public abstract void SetCiphertextFileName(DirectoryId directoryId, string cleartextFileName, string ciphertextFileName);

        protected record FileNameWithDirectoryId(DirectoryId DirectoryId, string FileName);
    }
}
