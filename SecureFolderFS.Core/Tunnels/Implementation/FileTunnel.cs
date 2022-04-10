using System;
using System.IO;
using System.Linq;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Sdk.Paths;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Sdk.Streams;

namespace SecureFolderFS.Core.Tunnels.Implementation
{
    internal sealed class FileTunnel : IFileTunnel
    {
        private readonly IFileOperations _fileOperations;

        private readonly IFileStreamReceiver _fileStreamReceiver;

        private readonly IPathReceiver _pathReceiver;

        public FileTunnel(IFileOperations fileOperations, IFileStreamReceiver fileStreamReceiver, IPathReceiver pathReceiver)
        {
            this._fileOperations = fileOperations;
            this._fileStreamReceiver = fileStreamReceiver;
            this._pathReceiver = pathReceiver;
        }

        public bool TransferFileToVault(string sourcePath, string destinationPath)
        {
            using Stream sourceStream = _fileOperations.OpenFile(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create the file and open stream
            ICiphertextPath ciphertextPath = _pathReceiver.FromCleartextPath<ICiphertextPath>(destinationPath);
            using var cleartextFileStream = _fileStreamReceiver.OpenFileStreamToCleartextFile(ciphertextPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, FileOptions.None);

            return WriteStreamsAndCompareHashes(sourceStream, cleartextFileStream.AsStream());
        }

        public bool TransferFileOutsideOfVault(ICleartextFileStream cleartextFileStream, string destinationPath)
        {
            using var destinationStream = _fileOperations.OpenFile(destinationPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return WriteStreamsAndCompareHashes(cleartextFileStream.AsStream(), destinationStream);
        }

        private static bool WriteStreamsAndCompareHashes(Stream sourceStream, Stream destinationStream)
        {
            // Write contents to file
            StreamHelpers.WriteToStream(sourceStream, destinationStream);

            // Reset position after writing streams
            sourceStream.Position = 0;
            destinationStream.Position = 0;

            // Calculate hashes
            byte[] sourceFileStreamHash = StreamHelpers.CalculateSha1Hash(sourceStream);
            byte[] destinationFileStreamHash = StreamHelpers.CalculateSha1Hash(destinationStream);

            // Compare to check integrity
            return sourceFileStreamHash.SequenceEqual(destinationFileStreamHash);
        }
    }
}
