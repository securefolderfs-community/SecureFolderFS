using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    internal sealed class EncryptingDavFolder<TCapability> : DavFolder<TCapability>
        where TCapability : IFolder
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly IDirectoryIdAccess _directoryIdAccess;

        /// <inheritdoc/>
        public override string Path => _pathConverter.ToCleartext(base.Path) ?? string.Empty;

        public EncryptingDavFolder(TCapability inner, IStreamsAccess streamsAccess, IPathConverter pathConverter, IDirectoryIdAccess directoryIdAccess)
            : base(inner)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdAccess = directoryIdAccess;
        }

        /// <inheritdoc/>
        public override async Task<IFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var formattedName = FormatName(desiredName);
            var folder = await modifiableFolder.CreateFolderAsync(formattedName, overwrite, cancellationToken);
            if (folder is not IModifiableFolder createdModifiableFolder)
                throw new ArgumentException("The created folder is not modifiable.");

            var dirIdFile = await createdModifiableFolder.CreateFileAsync(FileSystem.Constants.DIRECTORY_ID_FILENAME, false, cancellationToken);
            if (dirIdFile is not ILocatableFile locatableDirIdFile)
                throw new ArgumentException("The created directory ID file is not locatable.");

            _ = _directoryIdAccess.SetDirectoryId(locatableDirIdFile.Path, Guid.NewGuid().ToByteArray());
            return NewFolder(folder);
        }

        public override Task<IStorable> CreateCopyOfAsync(IStorable itemToCopy, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            // TODO: When copying, directory ID should be updated as well
            return base.CreateCopyOfAsync(itemToCopy, overwrite, cancellationToken);
        }

        /// <inheritdoc/>
        public override DavFile<T> NewFile<T>(T inner)
        {
            return new EncryptingDavFile<T>(inner, _streamsAccess, _pathConverter, _directoryIdAccess);
        }

        /// <inheritdoc/>
        public override DavFolder<T> NewFolder<T>(T inner)
        {
            return new EncryptingDavFolder<T>(inner, _streamsAccess, _pathConverter, _directoryIdAccess);
        }

        /// <inheritdoc/>
        protected override string FormatName(string name)
        {
            var cleartextPath = System.IO.Path.Combine(Path, name);
            return _pathConverter.GetCiphertextFileName(cleartextPath) ?? throw new CryptographicException("Couldn't convert to ciphertext path.");;
        }
    }
}
