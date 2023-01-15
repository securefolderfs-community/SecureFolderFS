using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    internal sealed class EncryptingDavFile<TCapability> : DavFile<TCapability>
        where TCapability : IFile
    {
        private readonly string _ciphertextPath;
        private readonly IStreamsAccess _streamsAccess;

        public EncryptingDavFile(TCapability storableInternal, string ciphertextPath, IStreamsAccess streamsAccess)
            : base(storableInternal)
        {
            _ciphertextPath = ciphertextPath;
            _streamsAccess = streamsAccess;
        }

        /// <inheritdoc/>
        public override async Task<Stream> OpenStreamAsync(FileAccess access, CancellationToken cancellationToken = default)
        {
            var stream = await base.OpenStreamAsync(access, cancellationToken);
            return OpenCleartextStream(stream);
        }

        /// <inheritdoc/>
        public override async Task<Stream> OpenStreamAsync(FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            var stream = await base.OpenStreamAsync(access, cancellationToken);
            return OpenCleartextStream(stream);
        }

        private Stream OpenCleartextStream(Stream ciphertextStream)
        {
            var cleartextStream = _streamsAccess.OpenCleartextStream(_ciphertextPath, ciphertextStream);
            _ = cleartextStream ?? throw new UnauthorizedAccessException("The cleartext stream couldn't be opened");

            return cleartextStream;
        }
    }
}
