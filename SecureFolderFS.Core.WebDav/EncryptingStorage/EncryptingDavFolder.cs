using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using SecureFolderFS.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
        private readonly DirectoryIdCache _directoryIdCache;

        /// <inheritdoc/>
        public override string Path => _pathConverter.ToCleartext(base.Path) ?? string.Empty;

        public EncryptingDavFolder(TCapability inner, IStreamsAccess streamsAccess, IPathConverter pathConverter, DirectoryIdCache directoryIdCache)
            : base(inner)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdCache = directoryIdCache;
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<INestedStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Inner.GetItemsAsync(kind, cancellationToken))
            {
                if (PathHelpers.IsCoreFile(item.Name))
                    continue;

                if (item is IFile file)
                    yield return NewFile(file);

                if (item is IFolder folder)
                    yield return NewFolder(folder);
            }
        }

        /// <inheritdoc/>
        public override async Task<INestedFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var formattedName = FormatName(desiredName);
            var folder = await modifiableFolder.CreateFolderAsync(formattedName, overwrite, cancellationToken);
            if (folder is not IModifiableFolder createdModifiableFolder)
                throw new ArgumentException("The created folder is not modifiable.");

            var dirIdFile = await createdModifiableFolder.CreateFileAsync(FileSystem.Constants.DIRECTORY_ID_FILENAME, false, cancellationToken);

            // Create new DirectoryID
            var directoryId = Guid.NewGuid().ToByteArray();

            // Initialize directory with DirectoryID
            await using var directoryIdStream = await dirIdFile.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
            await directoryIdStream.WriteAsync(directoryId, cancellationToken);

            // Set DirectoryID to known IDs
            if (dirIdFile is ILocatableFile locatableDirIdFile)
                _directoryIdCache.SetDirectoryId(locatableDirIdFile.Path, Guid.NewGuid().ToByteArray());

            return NewFolder(folder);
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

        /// <inheritdoc/>
        protected override string FormatName(string name)
        {
            var ciphertextPath = _pathConverter.ToCiphertext(System.IO.Path.Combine(Path, name));
            if (ciphertextPath is null)
                throw new CryptographicException("Couldn't convert to ciphertext path.");

            return System.IO.Path.GetFileName(ciphertextPath);
        }
    }
}
