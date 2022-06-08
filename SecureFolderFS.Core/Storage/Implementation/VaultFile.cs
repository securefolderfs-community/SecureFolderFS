using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Sdk.Streams;
using SecureFolderFS.Core.Streams.Receiver;

namespace SecureFolderFS.Core.Storage.Implementation
{
    internal sealed class VaultFile : VaultItem, IVaultFile
    {
        private readonly IFileStreamReceiver _fileStreamReceiver;

        public VaultFile(ICiphertextPath ciphertextPath, IFileSystemOperations fileSystemOperations, IFileStreamReceiver fileStreamReceiver)
            : base(ciphertextPath, fileSystemOperations)
        {
            _fileStreamReceiver = fileStreamReceiver;
        }

        public ICleartextFileStream OpenStream(FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            return _fileStreamReceiver.OpenFileStreamToCleartextFile(CiphertextPath, mode, access, share, options);
        }

        public override Task DeleteAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            //fileSystemOperations.DeleteFile(CiphertextPath); // TODO: Implement deletion

            return Task.CompletedTask;
        }
    }
}
