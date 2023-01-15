using NWebDav.Server.Enums;
using NWebDav.Server.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IDavFolder"/>
    /// <typeparam name="TCapability">An interface that represents capabilities of this folder.</typeparam>
    internal class DavFolder<TCapability> : DavStorable<IDavFolder, TCapability>, IDavFolder
        where TCapability : IFolder
    {
        /// <inheritdoc/>
        public string Path => Inner.TryGetPath() ?? string.Empty;

        /// <inheritdoc/>
        public EnumerationDepthMode DepthMode { get; } = EnumerationDepthMode.Assume0;

        /// <inheritdoc/>
        protected override IDavFolder Implementation => this;

        public DavFolder(TCapability storableInternal, IBasicProperties? properties = null)
            : base(storableInternal, properties)
        {
        }

        /// <inheritdoc/>
        public virtual async Task<IFile> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (Inner is not IFolderExtended folderExtended)
                throw new NotSupportedException("Retrieving individual files is not supported.");

            var file = await folderExtended.GetFileAsync(fileName, cancellationToken);
            return new DavFile<IFile>(file);
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder> GetFolderAsync(string folderName, CancellationToken cancellationToken = default)
        {
            if (Inner is not IFolderExtended folderExtended)
                throw new NotSupportedException("Retrieving individual folders is not supported.");

            var folder = await folderExtended.GetFolderAsync(folderName, cancellationToken);
            return new DavFolder<IFolder>(folder);
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<IStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Inner.GetItemsAsync(kind, cancellationToken))
            {
                if (item is IFile file)
                    yield return new DavFile<IFile>(file);

                if (item is IFolder folder)
                    yield return new DavFolder<IFolder>(folder);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            if (Inner is not ILocatableStorable locatableStorable)
                return null;

            var parentFolder = await locatableStorable.GetParentAsync(cancellationToken);
            if (parentFolder is null)
                return null;

            return new DavFolder<ILocatableFolder>(parentFolder);
        }

        /// <inheritdoc/>
        public virtual Task DeleteAsync(IStorable item, bool permanently = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            return modifiableFolder.DeleteAsync(item, permanently, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IStorable> CreateCopyOfAsync(IStorable itemToCopy, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var copiedItem = await modifiableFolder.CreateCopyOfAsync(itemToCopy, overwrite, cancellationToken);
            if (copiedItem is IFile file)
                return new DavFile<IFile>(file);

            if (copiedItem is IFolder folder)
                return new DavFolder<IFolder>(folder);

            throw new InvalidOperationException("The copied item is neither a file nor a folder.");
        }

        /// <inheritdoc/>
        public virtual async Task<IStorable> MoveFromAsync(IStorable itemToMove, IModifiableFolder source, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");
            
            var movedItem = await modifiableFolder.MoveFromAsync(itemToMove, source, overwrite, cancellationToken);
            if (movedItem is IFile file)
                return new DavFile<IFile>(file);

            if (movedItem is IFolder folder)
                return new DavFolder<IFolder>(folder);

            throw new InvalidOperationException("The moved item is neither a file nor a folder.");
        }

        /// <inheritdoc/>
        public virtual async Task<IFile> CreateFileAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var file = await modifiableFolder.CreateFileAsync(desiredName, overwrite, cancellationToken);
            return new DavFile<IFile>(file);
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var folder = await modifiableFolder.CreateFolderAsync(desiredName, overwrite, cancellationToken);
            return new DavFolder<IFolder>(folder);
        }
    }
}
