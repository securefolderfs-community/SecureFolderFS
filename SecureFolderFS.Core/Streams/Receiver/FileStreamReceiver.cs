using System;
using System.IO;
using System.Runtime.CompilerServices;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.FileSystem.OpenCryptoFiles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Sdk.Streams;

namespace SecureFolderFS.Core.Streams.Receiver
{
    internal sealed class FileStreamReceiver : IFileStreamReceiver
    {
        private readonly ISecurity _security;

        private readonly OpenCryptFileReceiver _openCryptFileReceiver;

        private readonly IChunkFactory _chunkFactory;

        private readonly IFileSystemOperations _fileSystemOperations;

        private bool _disposed;

        public FileStreamReceiver(ISecurity security, OpenCryptFileReceiver openCryptFileReceiver, IChunkFactory chunkFactory, IFileSystemOperations fileSystemOperations)
        {
            _security = security;
            _openCryptFileReceiver = openCryptFileReceiver;
            _chunkFactory = chunkFactory;
            _fileSystemOperations = fileSystemOperations;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ICleartextFileStream OpenFileStreamToCleartextFile(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            AssertNotDisposed();

            ICiphertextFileStream ciphertextFileStream = null;
            ICleartextFileStream cleartextFileStream = null;
            OpenCryptFile openCryptFile = null;

            try
            {
                ciphertextFileStream = OpenFileStreamToCiphertextFile(ciphertextPath, mode, access, FileShare.ReadWrite | FileShare.Delete, options); // TODO: share is not used

                var (fileHeader, isHeaderWritten) = GetFileHeader(ciphertextFileStream);

                openCryptFile = _openCryptFileReceiver.GetOrCreate(ciphertextPath, fileHeader);
                if (mode == FileMode.Truncate)
                {
                    openCryptFile.FlushChunkReceiver();
                }

                cleartextFileStream = new CleartextFileStream(
                    _security,
                    _fileSystemOperations,
                    _chunkFactory,
                    ciphertextFileStream,
                    fileHeader,
                    isHeaderWritten,
                    ciphertextPath,
                    mode,
                    access,
                    share
                    );

                openCryptFile.Open(cleartextFileStream, ciphertextFileStream);

                return cleartextFileStream;
            }
            catch
            {
                ciphertextFileStream?.Dispose();
                cleartextFileStream?.Dispose();
                openCryptFile?.Close(cleartextFileStream);

                throw;
            }
        }

        public ICiphertextFileStream OpenFileStreamToCiphertextFile(ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
        {
            AssertNotDisposed();

            return new CiphertextFileStream(ciphertextPath, mode, access, share);
        }

        private (IFileHeader fileHeader, bool isHeaderWritten) GetFileHeader(ICiphertextFileStream ciphertextFileStream)
        {
            if (ciphertextFileStream.Length == 0L)
            {
                return (_security.ContentCryptor.FileHeaderCryptor.CreateFileHeader(), false);
            }
            else
            {
                return (_security.ContentCryptor.FileHeaderCryptor.DecryptHeader(
                    _security.ContentCryptor.FileHeaderCryptor.CiphertextHeaderFromCiphertextFileStream(ciphertextFileStream)), true);
            }
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _openCryptFileReceiver.Dispose();
        }
    }
}
