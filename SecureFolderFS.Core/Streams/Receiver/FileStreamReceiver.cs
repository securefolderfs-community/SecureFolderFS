using SecureFolderFS.Core.FileSystem.OpenCryptoFiles;
using SecureFolderFS.Core.Security;
using System;
using System.IO;

namespace SecureFolderFS.Core.Streams.Receiver
{
    internal sealed class FileStreamReceiver : IFileStreamReceiver
    {
        private readonly ISecurity _security;
        private readonly OpenCryptFileReceiver _openCryptFileReceiver;

        public FileStreamReceiver(ISecurity security, OpenCryptFileReceiver openCryptFileReceiver)
        {
            _security = security;
            _openCryptFileReceiver = openCryptFileReceiver;
        }

        /// <inheritdoc/>
        public Stream OpenCleartextStream(string ciphertextPath, Stream ciphertextStream)
        {
            OpenCryptFile? openCryptFile = null;
            CleartextFileStream? cleartextStream = null;
            try
            {
                openCryptFile = _openCryptFileReceiver.TryGet(ciphertextPath) ?? _openCryptFileReceiver.Create(ciphertextPath, new(_security.HeaderCrypt.HeaderCleartextSize));
                cleartextStream = new CleartextFileStream(_security, openCryptFile.ChunkAccess, ciphertextStream, openCryptFile.FileHeader);

                openCryptFile.RegisterStream(cleartextStream, ciphertextStream);

                return cleartextStream;
            }
            catch (Exception)
            {
                ciphertextStream.Dispose();
                cleartextStream?.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _openCryptFileReceiver.Dispose();
        }
    }
}
