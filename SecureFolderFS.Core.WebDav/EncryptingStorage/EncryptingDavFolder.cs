using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage.Extensions;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage
{
    internal sealed class EncryptingDavFolder<TCapability> : DavFolder<TCapability>
        where TCapability : IFolder
    {
        private readonly IStreamsAccess _streamsAccess;
        private readonly IPathConverter _pathConverter;
        private readonly IDirectoryIdAccess _directoryIdAccess;

        /// <inheritdoc/>
        // base.Path returns string.Empty for some reason if the folder is inside another folder
        public override string Path => _pathConverter.ToCleartext(Inner.TryGetPath() ?? string.Empty) ?? string.Empty;

        public EncryptingDavFolder(TCapability inner, IStreamsAccess streamsAccess, IPathConverter pathConverter, IDirectoryIdAccess directoryIdAccess)
            : base(inner)
        {
            _streamsAccess = streamsAccess;
            _pathConverter = pathConverter;
            _directoryIdAccess = directoryIdAccess;
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<IStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

        /// <inheritdoc/>
        public override Task<IStorable> CreateCopyOfAsync(IStorable itemToCopy, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            _ = itemToCopy;
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
            var ciphertextPath = _pathConverter.ToCiphertext(System.IO.Path.Combine(Path, name));
            if (ciphertextPath is null)
                throw new CryptographicException("Couldn't convert to ciphertext path.");

            return System.IO.Path.GetFileName(ciphertextPath);
        }
    }
}
