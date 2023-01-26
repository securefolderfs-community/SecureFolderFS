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
        public virtual string Path => Inner.TryGetPath() ?? string.Empty;

        /// <inheritdoc/>
        public virtual EnumerationDepthMode DepthMode { get; } = EnumerationDepthMode.Assume0;

        /// <inheritdoc/>
        protected override IDavFolder Implementation => this;

        public DavFolder(TCapability inner)
            : base(inner)
        {
        }

        public DavFolder(TCapability inner, IBasicProperties properties)
            : base(inner, properties)
        {
        }

        /// <inheritdoc/>
        public virtual async Task<IFile> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (Inner is not IFolderExtended folderExtended)
                throw new NotSupportedException("Retrieving individual files is not supported.");

            var formattedName = FormatName(fileName);
            var file = await folderExtended.GetFileAsync(formattedName, cancellationToken);

            return NewFile(file);
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder> GetFolderAsync(string folderName, CancellationToken cancellationToken = default)
        {
            if (Inner is not IFolderExtended folderExtended)
                throw new NotSupportedException("Retrieving individual folders is not supported.");

            var formattedName = FormatName(folderName);
            var folder = await folderExtended.GetFolderAsync(formattedName, cancellationToken);

            return NewFolder(folder);
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<IStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Inner.GetItemsAsync(kind, cancellationToken))
            {
                if (item is IFile file)
                    yield return NewFile(file);

                if (item is IFolder folder)
                    yield return NewFolder(folder);
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

            return NewFolder(parentFolder);
        }

        /// <inheritdoc/>
        public virtual Task DeleteAsync(IStorable item, bool permanently = default, CancellationToken cancellationToken = default)
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
                return NewFile(file);

            if (copiedItem is IFolder folder)
                return NewFolder(folder);

            throw new InvalidOperationException("The copied item is neither a file nor a folder.");
        }

        /// <inheritdoc/>
        public virtual async Task<IStorable> MoveFromAsync(IStorable itemToMove, IModifiableFolder source, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");
            
            var movedItem = await modifiableFolder.MoveFromAsync(itemToMove, source, overwrite, cancellationToken);
            if (movedItem is IFile file)
                return NewFile(file);

            if (movedItem is IFolder folder)
                return NewFolder(folder);

            throw new InvalidOperationException("The moved item is neither a file nor a folder.");
        }

        /// <inheritdoc/>
        public virtual async Task<IFile> CreateFileAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var formattedName = FormatName(desiredName);
            var file = await modifiableFolder.CreateFileAsync(formattedName, overwrite, cancellationToken);

            return NewFile(file);
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var formattedName = FormatName(desiredName);
            var folder = await modifiableFolder.CreateFolderAsync(formattedName, overwrite, cancellationToken);

            return NewFolder(folder);
        }

        /// <summary>
        /// Formats specified <paramref name="name"/> if necessary.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <returns>A formatted name which can be used to communicate with additional abstraction layer.</returns>
        protected virtual string FormatName(string name)
        {
            return name;
        }
    }
}
