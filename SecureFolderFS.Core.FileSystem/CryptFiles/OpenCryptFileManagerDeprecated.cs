using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Core.FileSystem.CryptFiles
{
    public class OpenCryptFileManagerDeprecated : IDisposable
    {
        protected readonly IChunkAccessInitializer chunkAccessInitializer;
        protected readonly IStreamsManagerInitializer streamsManagerInitializer;
        protected readonly Dictionary<string, IOpenCryptFile> openCryptFiles;

        public OpenCryptFileManagerDeprecated(IChunkAccessInitializer chunkAccessInitializer, IStreamsManagerInitializer streamsManagerInitializer)
        {
            this.chunkAccessInitializer = chunkAccessInitializer;
            this.streamsManagerInitializer = streamsManagerInitializer;
            this.openCryptFiles = new();
        }

        public virtual IOpenCryptFile? TryGet(string ciphertextPath)
        {
            openCryptFiles.TryGetValue(ciphertextPath, out var openCryptFile);
            return openCryptFile;
        }

        public virtual IOpenCryptFile CreateNew(string ciphertextPath, BufferHolder headerBuffer)
        {
            var streamsManager = streamsManagerInitializer.GetStreamsManager();
            var chunkAccess = GetChunkAccess(streamsManager, headerBuffer);

            var openCryptFile = new IOpenCryptFile();
            openCryptFiles.Add(ciphertextPath, openCryptFile);

            return openCryptFile;
        }

        protected virtual IChunkAccess GetChunkAccess(IStreamsManager streamsManager, BufferHolder headerBuffer)
        {
            var reader = chunkAccessInitializer.GetChunkReader(streamsManager, headerBuffer);
            var writer = chunkAccessInitializer.GetChunkWriter(streamsManager, headerBuffer);

            return chunkAccessInitializer.GetChunkAccess(reader, writer);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            openCryptFiles.Values.DisposeCollection();
            openCryptFiles.Clear();
        }
    }
}
