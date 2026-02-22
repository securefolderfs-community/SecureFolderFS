using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Core.FileSystem.Storage.StorageProperties;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Recyclable;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    /// <inheritdoc cref="IFolder"/>
    public class CryptoFolder : CryptoStorable<IFolder>, IChildFolder, IGetFirstByName, IMoveRenamedFrom, ICreateRenamedCopyOf, IRenamableFolder, IRecyclableFolder
    {
        public CryptoFolder(string plaintextId, IFolder inner, FileSystemSpecifics specifics, CryptoFolder? parent = null)
            : base(plaintextId, inner, specifics, parent)
        {
        }

        /// <inheritdoc/>
        public virtual async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            if (Inner is not IRenamableFolder renamableFolder)
                throw new NotSupportedException("Renaming folder contents is not supported.");

            // We need to get the equivalent on the disk
            var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(storable.Name, Inner, specifics, cancellationToken);
            var ciphertextItem = await Inner.GetFirstByNameAsync(ciphertextName, cancellationToken);

            // Encrypt name
            var newCiphertextName = await AbstractPathHelpers.EncryptNameAsync(newName, Inner, specifics, cancellationToken);
            var renamedCiphertextItem = await renamableFolder.RenameAsync(ciphertextItem, newCiphertextName, cancellationToken);

            var plaintextId = Path.Combine(Inner.Id, newName);
            return renamedCiphertextItem switch
            {
                IFile file => new CryptoFile(plaintextId, file, specifics, this),
                IFolder folder => new CryptoFolder(plaintextId, folder, specifics, this),
                _ => throw new ArgumentOutOfRangeException(nameof(renamedCiphertextItem))
            };
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in Inner.GetItemsAsync(type, cancellationToken))
            {
                if (PathHelpers.IsCoreName(item.Name))
                    continue;

                var plaintextName = await AbstractPathHelpers.DecryptNameAsync(item.Name, Inner, specifics, cancellationToken);
                if (plaintextName is null)
                    continue;

                yield return item switch
                {
                    IFile file => (IStorableChild)Wrap(file, plaintextName),
                    IFolder folder => (IStorableChild)Wrap(folder, plaintextName),
                    _ => throw new InvalidOperationException("The enumerated item was neither a file nor a folder.")
                };
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(name, Inner, specifics, cancellationToken);
            return await Inner.GetFirstByNameAsync(ciphertextName, cancellationToken) switch
            {
                IChildFile file => (IStorableChild)Wrap(file, name),
                IChildFolder folder => (IStorableChild)Wrap(folder, name),
                _ => throw new InvalidCastException("Could not match the item to neither a file nor a folder.")
            };
        }

        /// <inheritdoc/>
        public virtual Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            // TODO(ns): Implement FolderWatcher for CryptoFolder
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(item, -1L, false, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(IStorableChild item, long sizeHint = -1L, bool deleteImmediately = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            // We need to get the equivalent on the disk
            var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(item.Name, Inner, specifics, cancellationToken);
            var ciphertextItem = await Inner.GetFirstByNameAsync(ciphertextName, cancellationToken);

            if (deleteImmediately)
            {
                // Delete the ciphertext item
                await modifiableFolder.DeleteAsync(ciphertextItem, cancellationToken);
            }
            else
            {
                // Delete or recycle the ciphertext item
                await AbstractRecycleBinHelpers.DeleteOrRecycleAsync(modifiableFolder, ciphertextItem, specifics, StreamSerializer.Instance, sizeHint, cancellationToken: cancellationToken);
            }

            // Remove deleted directory from cache
            if (ciphertextItem is IFolder)
                specifics.DirectoryIdCache.CacheRemove(Path.Combine(ciphertextItem.Id, Constants.Names.DIRECTORY_ID_FILENAME));
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var encryptedName = await AbstractPathHelpers.EncryptNameAsync(name, Inner, specifics, cancellationToken);
            var folder = await modifiableFolder.CreateFolderAsync(encryptedName, overwrite, cancellationToken);
            if (folder is not IModifiableFolder createdModifiableFolder)
                throw new ArgumentException("The created folder is not modifiable.");

            // Get the DirectoryID file
            var directoryIdFile = await createdModifiableFolder.CreateFileAsync(Constants.Names.DIRECTORY_ID_FILENAME, false, cancellationToken);
            await using var directoryIdStream = await directoryIdFile.OpenStreamAsync(FileAccess.Write, cancellationToken);

            // Initialize directory with Directory ID
            var directoryId = Guid.NewGuid().ToByteArray();
            await directoryIdStream.WriteAsync(directoryId, cancellationToken);

            // Set DirectoryID to known IDs
            specifics.DirectoryIdCache.CacheSet(directoryIdFile.Id, new(directoryId));

            return (IChildFolder)Wrap(folder, name);
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var encryptedName = await AbstractPathHelpers.EncryptNameAsync(name, Inner, specifics, cancellationToken);
            var file = await modifiableFolder.CreateFileAsync(encryptedName, overwrite, cancellationToken);

            return (IChildFile)Wrap(file, name);
        }

        /// <inheritdoc/>
        public virtual Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, CancellationToken cancellationToken,
            CreateCopyOfDelegate fallback)
        {
            return CreateCopyOfAsync(fileToCopy, overwrite, fileToCopy.Name, cancellationToken, (mf, f, ov, _, ct) => fallback(mf, f, ov, ct));
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, string newName, CancellationToken cancellationToken,
            CreateRenamedCopyOfDelegate fallback)
        {
            if (Inner is not IModifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            if (Inner is not ICreateRenamedCopyOf createRenamedCopyOf || fileToCopy is not IChildFile fileToCopyChild)
                return await fallback(this, fileToCopy, overwrite, newName, cancellationToken);

            // Get the ciphertext representation of the file to copy
            var ciphertextFileToCopy = await GetCiphertextRepresentationAsync(fileToCopyChild, cancellationToken);
            if (ciphertextFileToCopy is null)
                return await fallback(this, fileToCopy, overwrite, newName, cancellationToken);

            // Encrypt the new name
            var ciphertextNewName = await AbstractPathHelpers.EncryptNameAsync(newName, Inner, specifics, cancellationToken);

            // Copy the ciphertext file
            var copiedCiphertextFile = await createRenamedCopyOf.CreateCopyOfAsync(ciphertextFileToCopy, overwrite, ciphertextNewName, cancellationToken);
            return (IChildFile)Wrap(copiedCiphertextFile, newName);
        }

        /// <inheritdoc/>
        public virtual Task<IChildFile> MoveFromAsync(IChildFile fileToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken,
            MoveFromDelegate fallback)
        {
            return MoveFromAsync(fileToMove, source, overwrite, fileToMove.Name, cancellationToken, (mf, f, src, ov, _, ct) => fallback(mf, f, src, ov, ct));
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFile> MoveFromAsync(IChildFile fileToMove, IModifiableFolder source, bool overwrite, string newName,
            CancellationToken cancellationToken, MoveRenamedFromDelegate fallback)
        {
            if (Inner is not IModifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            if (source is not IChildFolder sourceFolder || Inner is not IMoveRenamedFrom moveRenamedFrom)
                return await fallback(this, fileToMove, source, overwrite, newName, cancellationToken);

            // Get the ciphertext representation of the source folder
            var ciphertextSource = await GetCiphertextRepresentationAsync(sourceFolder, cancellationToken);
            if (ciphertextSource is not IModifiableFolder ciphertextSourceModifiableFolder)
                return await fallback(this, fileToMove, source, overwrite, newName, cancellationToken);

            // Get the ciphertext representation of the file to move
            var existingCiphertextName = await AbstractPathHelpers.EncryptNameAsync(fileToMove.Name, ciphertextSource, specifics, cancellationToken);
            var ciphertextFileToMove = await ciphertextSource.TryGetFileByNameAsync(existingCiphertextName, cancellationToken);
            if (ciphertextFileToMove is null)
                return await fallback(this, fileToMove, source, overwrite, newName, cancellationToken);

            // Encrypt the new name
            var newCiphertextName = await AbstractPathHelpers.EncryptNameAsync(newName, Inner, specifics, cancellationToken);

            // Move the ciphertext file
            var movedCiphertextFile = await moveRenamedFrom.MoveFromAsync(ciphertextFileToMove, ciphertextSourceModifiableFolder, overwrite, newCiphertextName, cancellationToken, fallback);
            return (IChildFile)Wrap(movedCiphertextFile, newName);
        }

        /// <inheritdoc/>
        public override async Task<IBasicProperties> GetPropertiesAsync()
        {
            if (Inner is not IStorableProperties storableProperties)
                throw new NotSupportedException($"Properties on {nameof(CryptoFolder)}.{nameof(Inner)} are not supported.");

            var innerProperties = await storableProperties.GetPropertiesAsync();
            properties ??= new CryptoFolderProperties(innerProperties);

            return properties;
        }

        /// <summary>
        /// Retrieves the ciphertext representation of a given storable child item if available.
        /// </summary>
        /// <typeparam name="TStorable">The type of the storable item, which must implement <see cref="IStorableChild"/>.</typeparam>
        /// <param name="item">The storable child item to retrieve the ciphertext representation for.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the ciphertext representation of the <paramref name="item"/> as <typeparamref name="TStorable"/>.</returns>
        protected virtual async Task<TStorable?> GetCiphertextRepresentationAsync<TStorable>(TStorable item,
            CancellationToken cancellationToken)
            where TStorable : class, IStorableChild
        {
            var parentFolder = await item.GetParentAsync(cancellationToken);
            if (parentFolder is null || parentFolder.Id == Path.DirectorySeparatorChar.ToString())
            {
                // We're at the root
                parentFolder ??= item as IFolder;
                if (parentFolder is not IWrapper<IFolder> folderWrapper)
                    return null;

                if (folderWrapper.GetWrapperAt<IFolder, CryptoFolder>() is not { Inner: var ciphertextRoot })
                    return null;

                var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(item.Name, ciphertextRoot, specifics, cancellationToken);
                return await ciphertextRoot.TryGetFirstByNameAsync(ciphertextName, cancellationToken) as TStorable;
            }

            if (parentFolder is not IWrapper<IFolder> parentFolderWrapper)
                return null;

            if (parentFolderWrapper.GetWrapperAt<IFolder, CryptoFolder>() is not { Inner: var ciphertextParent })
                return null;

            var ciphertextName2 = await AbstractPathHelpers.EncryptNameAsync(item.Name, ciphertextParent, specifics, cancellationToken);
            return await ciphertextParent.TryGetFirstByNameAsync(ciphertextName2, cancellationToken) as TStorable;
        }
    }
}
