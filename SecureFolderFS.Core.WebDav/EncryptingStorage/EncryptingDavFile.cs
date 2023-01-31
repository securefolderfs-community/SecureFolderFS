using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Directories;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    /// <inheritdoc cref="DavFile{TCapability}"/>
    internal sealed class EncryptingDavFile<TCapability> : DavFile<TCapability>
        where TCapability : IFile
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly IDirectoryIdAccess _directoryIdAccess;

        /// <inheritdoc/>
        public override string Path => _pathConverter.ToCleartext(base.Path) ?? string.Empty;

        public EncryptingDavFile(TCapability inner, IStreamsAccess streamsAccess, IPathConverter pathConverter, IDirectoryIdAccess directoryIdAccess)
            : base(inner)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdAccess = directoryIdAccess;
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

        /// <inheritdoc/>
        public override DavFolder<T> NewFolder<T>(T inner)
        {
            return new EncryptingDavFolder<T>(inner, _streamsAccess, _pathConverter, _directoryIdAccess);
        }

        private Stream OpenCleartextStream(Stream ciphertextStream)
        {
            if (Inner is not ILocatableFile locatableFile)
                throw new NotSupportedException($"{nameof(Inner)} is not locatable.");

            var cleartextStream = _streamsAccess.OpenCleartextStream(locatableFile.Path, ciphertextStream);
            _ = cleartextStream ?? throw new UnauthorizedAccessException("The cleartext stream couldn't be opened");

            return cleartextStream;
        }
    }
}
