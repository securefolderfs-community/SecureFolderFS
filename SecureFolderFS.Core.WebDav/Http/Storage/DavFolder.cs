using NWebDav.Server.Enums;
using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Storage
{
    /// <inheritdoc cref="IDavFolder"/>
    internal sealed class DavFolder : DavStorable<IDavFolder, ILocatableFolder>, IDavFolder
    {
        /// <inheritdoc/>
        public string Path { get; }

        /// <inheritdoc/>
        public EnumerationDepthMode DepthMode { get; } = EnumerationDepthMode.Assume0;

        /// <inheritdoc/>
        protected override IDavFolder Implementation => this;

        public DavFolder(ILocatableFolder storableInternal, IBasicProperties? properties)
            : base(storableInternal, properties)
        {
            Path = storableInternal.Path;
        }

        /// <inheritdoc/>
        public Task<IFile> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return StorableInternal.GetFileAsync(fileName, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IFolder> GetFolderAsync(string folderName, CancellationToken cancellationToken = default)
        {
            return StorableInternal.GetFolderAsync(folderName, cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IStorable> GetItemsAsync(StorableKind kind = StorableKind.All, CancellationToken cancellationToken = default)
        {
            return StorableInternal.GetItemsAsync(kind, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return StorableInternal.GetParentAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task DeleteAsync(IStorable item, bool permanently = false, CancellationToken cancellationToken = default)
        {
            if (StorableInternal is IModifiableFolder modifiableFolder)
                return modifiableFolder.DeleteAsync(item, permanently, cancellationToken);

            throw new NotSupportedException("Modifying folder contents is not supported.");
        }

        /// <inheritdoc/>
        public Task<IStorable> CreateCopyOfAsync(IStorable itemToCopy, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            if (StorableInternal is IModifiableFolder modifiableFolder)
                return modifiableFolder.CreateCopyOfAsync(itemToCopy, collisionOption, cancellationToken);

            throw new NotSupportedException("Modifying folder contents is not supported.");
        }

        /// <inheritdoc/>
        public Task<IStorable> MoveFromAsync(IStorable itemToMove, IModifiableFolder source, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            if (StorableInternal is IModifiableFolder modifiableFolder)
                return modifiableFolder.MoveFromAsync(itemToMove, source, collisionOption, cancellationToken);

            throw new NotSupportedException("Modifying folder contents is not supported.");
        }

        /// <inheritdoc/>
        public async Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            if (StorableInternal is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var file = await modifiableFolder.CreateFileAsync(desiredName, collisionOption, cancellationToken);
            if (file is not ILocatableFile locatableFile)
                throw new NotSupportedException("The created file is not locatable.");

            return new DavFile(locatableFile, null);
        }

        /// <inheritdoc/>
        public async Task<IFolder> CreateFolderAsync(string desiredName, CreationCollisionOption collisionOption = default, CancellationToken cancellationToken = default)
        {
            if (StorableInternal is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var folder = await modifiableFolder.CreateFolderAsync(desiredName, collisionOption, cancellationToken);
            if (folder is not ILocatableFolder locatableFolder)
                throw new NotSupportedException("The created file is not locatable.");

            return new DavFolder(locatableFolder, null);
        }
    }
}
