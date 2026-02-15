using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Core.FileSystem.Storage.StorageProperties;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Recyclable;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.StorageProperties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Core.FileSystem.Storage
{
    // TODO(ns): Add move and copy support
    /// <inheritdoc cref="IFolder"/>
    public class CryptoFolder : CryptoStorable<IFolder>, IChildFolder, IGetFirstByName, IRenamableFolder, IRecyclableFolder, ICreateCopyOf
    {
        public CryptoFolder(string plaintextId, IFolder inner, FileSystemSpecifics specifics, CryptoFolder? parent = null)
            : base(plaintextId, inner, specifics, parent)
        {
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
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
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
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
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            // TODO(ns): Implement FolderWatcher for CryptoFolder
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            await DeleteAsync(item, -1L, false, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, long sizeHint, bool deleteImmediately = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            // We need to get the equivalent on the disk
            var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(item.Name, Inner, specifics, cancellationToken);
            var ciphertextItem = await Inner.GetFirstByNameAsync(ciphertextName, cancellationToken);

            if (deleteImmediately)
            {
                // Delete the ciphertext item
                await modifiableFolder.DeleteAsync(item, cancellationToken);
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
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
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

            // Initialize directory with DirectoryID
            var directoryId = Guid.NewGuid().ToByteArray();
            await directoryIdStream.WriteAsync(directoryId, cancellationToken);

            // Set DirectoryID to known IDs
            specifics.DirectoryIdCache.CacheSet(directoryIdFile.Id, new(directoryId));

            return (IChildFolder)Wrap(folder, name);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            var encryptedName = await AbstractPathHelpers.EncryptNameAsync(name, Inner, specifics, cancellationToken);
            var file = await modifiableFolder.CreateFileAsync(encryptedName, overwrite, cancellationToken);

            return (IChildFile)Wrap(file, name);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, CancellationToken cancellationToken,
            CreateCopyOfDelegate fallback)
        {
            if (Inner is not IModifiableFolder modifiableFolder)
                throw new NotSupportedException("Modifying folder contents is not supported.");

            if (fileToCopy is not IChildFile fileToCopyChild || Inner is not IRenamableFolder renamableFolder || Inner is not ICreateCopyOf createCopyOf)
                return await fallback(this, fileToCopy, overwrite, cancellationToken);

            // Get destination name and check if it already exists
            var destinationCiphertextName = await AbstractPathHelpers.EncryptNameAsync(fileToCopy.Name, Inner, specifics, cancellationToken);
            if (await Inner.TryGetFirstByNameAsync(destinationCiphertextName, cancellationToken) is { } existingItem)
            {
                if (!overwrite)
                    throw new FileAlreadyExistsException(fileToCopy.Name);

                await modifiableFolder.DeleteAsync(existingItem, cancellationToken);
            }

            if (await fileToCopyChild.GetParentAsync(cancellationToken) is IWrapper<IFolder> { Inner: { } shallowInnerParentOfFileToCopy})
            {
                // We're copying other CryptoStorable

                // Get the internal ciphertext file to copy
                var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(fileToCopy.Name, shallowInnerParentOfFileToCopy, specifics, cancellationToken);
                var ciphertextItem = await shallowInnerParentOfFileToCopy.GetFileByNameAsync(ciphertextName, cancellationToken);

                // Copy and rename the file
                var copiedCiphertextFile = await createCopyOf.CreateCopyOfAsync(ciphertextItem, overwrite, cancellationToken, fallback);
                var renamedCiphertextFile = (IChildFile)await renamableFolder.RenameAsync(copiedCiphertextFile, destinationCiphertextName, cancellationToken);

                return (IChildFile)Wrap(renamedCiphertextFile, fileToCopy.Name);
            }
            else
            {
                // We're copying a file assumed to be from a different storage source

                // Copy the file and recreate the actual encrypted file
                var copiedPlaintextFile = await createCopyOf.CreateCopyOfAsync(fileToCopy, overwrite, cancellationToken, fallback);
                var createdCiphertextFile = await CreateFileAsync(fileToCopy.Name, overwrite, cancellationToken);

                // Copy and encrypt the contents
                await using var copiedPlaintextFileStream = await copiedPlaintextFile.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
                await using var createdCiphertextFileStream = await createdCiphertextFile.OpenStreamAsync(FileAccess.Write, cancellationToken);
                await copiedPlaintextFileStream.CopyToAsync(createdCiphertextFileStream, cancellationToken);

                // Delete the old plaintext file
                await modifiableFolder.DeleteAsync(copiedPlaintextFile, cancellationToken);

                return (IChildFile)Wrap(createdCiphertextFile, fileToCopy.Name);
            }
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
    }
}
