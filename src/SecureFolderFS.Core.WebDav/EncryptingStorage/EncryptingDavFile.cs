using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    /// <inheritdoc cref="DavFile{TCapability}"/>
    internal sealed class EncryptingDavFile<TCapability> : DavFile<TCapability>
        where TCapability : IFile
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly DirectoryIdCache _directoryIdCache;

        /// <inheritdoc/>
        public override string Path => _pathConverter.ToCleartext(base.Path) ?? string.Empty;

        public EncryptingDavFile(TCapability inner, IStreamsAccess streamsAccess, IPathConverter pathConverter, DirectoryIdCache directoryIdCache)
            : base(inner)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdCache = directoryIdCache;
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
            var stream = await base.OpenStreamAsync(access, share, cancellationToken);
            return OpenCleartextStream(stream);
        }

        /// <inheritdoc/>
        public override IWrapper<IFile> Wrap(IFile file)
        {
            return new EncryptingDavFile<IFile>(file, _streamsAccess, _pathConverter, _directoryIdCache);
        }

        /// <inheritdoc/>
        public override IWrapper<IFolder> Wrap(IFolder folder)
        {
            return new EncryptingDavFolder<IFolder>(folder, _streamsAccess, _pathConverter, _directoryIdCache);
        }

        private Stream OpenCleartextStream(Stream ciphertextStream)
        {
            if (Inner is not ILocatableFile locatableFile)
                throw new NotSupportedException($"{nameof(Inner)} is not locatable.");

            var cleartextStream = _streamsAccess.OpenCleartextStream(locatableFile.Path, ciphertextStream);
            return cleartextStream;
        }
    }
}
