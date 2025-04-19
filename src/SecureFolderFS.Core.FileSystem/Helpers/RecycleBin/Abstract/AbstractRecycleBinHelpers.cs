using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static class AbstractRecycleBinHelpers
    {
        public static async Task<long> GetOccupiedSizeAsync(IModifiableFolder recycleBin, CancellationToken cancellationToken = default)
        {
            var recycleBinConfig = await recycleBin.CreateFileAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, false, cancellationToken);
            var text = await recycleBinConfig.ReadAllTextAsync(null, cancellationToken);
            if (!long.TryParse(text, out var value))
                return 0L;

            return Math.Max(0L, value);
        }

        public static async Task SetOccupiedSizeAsync(IModifiableFolder recycleBin, long value, CancellationToken cancellationToken = default)
        {
            var recycleBinConfig = await recycleBin.CreateFileAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, false, cancellationToken);
            await recycleBinConfig.WriteAllTextAsync(Math.Max(0L, value).ToString(), null, cancellationToken);
        }

        public static async Task<RecycleBinItemDataModel> GetItemDataModelAsync(IStorableChild item, IFolder recycleBin, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Read configuration file
            var configurationFile = await recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken);
            await using var configurationStream = await configurationFile.OpenReadAsync(cancellationToken);

            // Deserialize configuration
            var deserialized = await streamSerializer.DeserializeAsync<Stream, RecycleBinItemDataModel>(configurationStream, cancellationToken);
            if (deserialized is not { ParentPath: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            return deserialized;
        }

        public static async Task<IModifiableFolder?> GetDestinationFolderAsync(IStorableChild item, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Get recycle bin
            var recycleBin = await GetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is null)
                throw new DirectoryNotFoundException("Could not find recycle bin folder.");

            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(item, recycleBin, streamSerializer, cancellationToken);
            if (deserialized is not { ParentPath: not null, OriginalName: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            // Check if destination item exists
            var parentId = deserialized.ParentPath.Replace('/', Path.DirectorySeparatorChar);
            var itemId = Path.Combine(parentId, deserialized.OriginalName);
            try
            {
                _ = await specifics.ContentFolder.GetItemByRelativePathOrSelfAsync(itemId, cancellationToken);

                // Destination item already exists, user must choose a new location
                return null;
            }
            catch (Exception) { }

            // Check if destination folder exists
            try
            {
                var parentItem = await specifics.ContentFolder.GetItemByRelativePathOrSelfAsync(parentId, cancellationToken);

                // Assume the parent is a folder and return it
                return parentItem as IModifiableFolder;
            }
            catch (Exception) { }

            // No destination folder has been found, user must choose a new location
            return null;
        }

        public static async Task RestoreAsync(IStorableChild item, IModifiableFolder destinationFolder, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // Get recycle bin
            var recycleBin = await GetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IRenamableFolder renamableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not renamable.");

            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(item, recycleBin, streamSerializer, cancellationToken);
           
            // Rename the item to correct name
            _ = deserialized.OriginalName ?? throw new IOException("Could not get file name.");
            var renamedItem = await renamableRecycleBin.RenameAsync(item, deserialized.OriginalName, cancellationToken);

            // Move item to destination
            _ = await destinationFolder.MoveStorableFromAsync(renamedItem, renamableRecycleBin, false, cancellationToken);

            // Delete old configuration file
            var configurationFile = await recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken);
            await renamableRecycleBin.DeleteAsync(configurationFile, cancellationToken);

            // Check if the item had any size
            if (deserialized.Size is not ({ } size and > 0L))
                return;

            // Update occupied size
            var occupiedSize = await GetOccupiedSizeAsync(renamableRecycleBin, cancellationToken);
            var newSize = occupiedSize - size;
            await SetOccupiedSizeAsync(renamableRecycleBin, newSize, cancellationToken);
        }

        public static async Task DeleteOrRecycleAsync(
            IModifiableFolder sourceFolder,
            IStorableChild item,
            FileSystemSpecifics specifics,
            IAsyncSerializer<Stream> streamSerializer,
            long sizeHint = -1L,
            CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            if (!specifics.Options.IsRecycleBinEnabled())
            {
                await sourceFolder.DeleteAsync(item, cancellationToken);
                return;
            }

            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IRenamableFolder renamableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not renamable.");

            if (sizeHint < 0 && specifics.Options.RecycleBinSize > 0L)
            {
                sizeHint = item switch
                {
                    IFile file => await file.GetSizeAsync(cancellationToken),
                    IFolder folder => await folder.GetSizeAsync(cancellationToken),
                    _ => 0L
                };

                var occupiedSize = await GetOccupiedSizeAsync(renamableRecycleBin, cancellationToken);
                var availableSize = specifics.Options.RecycleBinSize - occupiedSize;
                if (availableSize < sizeHint)
                {
                    await sourceFolder.DeleteAsync(item, cancellationToken);
                    return;
                }
            }

            // Get source Directory ID
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, sourceFolder.Id);
            var directoryIdResult = await AbstractPathHelpers.GetDirectoryIdAsync(sourceFolder, specifics, directoryId, cancellationToken);

            // Move and rename item
            var guid = Guid.NewGuid().ToString();
            var movedItem = await renamableRecycleBin.MoveStorableFromAsync(item, sourceFolder, false, cancellationToken);
            _ = await renamableRecycleBin.RenameAsync(movedItem, guid, cancellationToken);

            // Create configuration file
            var configurationFile = await renamableRecycleBin.CreateFileAsync($"{guid}.json", false, cancellationToken);
            await using var configurationStream = await configurationFile.OpenReadWriteAsync(cancellationToken);

            // Serialize configuration data model
            await using var serializedStream = await streamSerializer.SerializeAsync(
                new RecycleBinItemDataModel()
                {
                    OriginalName = item.Name,
                    ParentPath = sourceFolder.Id.Replace(specifics.ContentFolder.Id, string.Empty).Replace(Path.DirectorySeparatorChar, '/'),
                    DirectoryId = directoryIdResult ? directoryId : [],
                    DeletionTimestamp = DateTime.Now,
                    Size = sizeHint
                }, cancellationToken);

            // Write to destination stream
            await serializedStream.CopyToAsync(configurationStream, cancellationToken);

            // Update occupied size
            if (specifics.Options.IsRecycleBinEnabled())
            {
                var occupiedSize = await GetOccupiedSizeAsync(renamableRecycleBin, cancellationToken);
                var newSize = occupiedSize + sizeHint;
                await SetOccupiedSizeAsync(renamableRecycleBin, newSize, cancellationToken);
            }
        }

        public static async Task<IFolder?> GetRecycleBinAsync(FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            try
            {
                return await specifics.ContentFolder.GetFolderByNameAsync(Constants.Names.RECYCLE_BIN_NAME, cancellationToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<IFolder> GetOrCreateRecycleBinAsync(FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            var recycleBin = await GetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not null)
                return recycleBin;

            if (specifics.ContentFolder is not IModifiableFolder modifiableFolder)
                throw new UnauthorizedAccessException("The content folder is not modifiable.");

            return await modifiableFolder.CreateFolderAsync(Constants.Names.RECYCLE_BIN_NAME, false, cancellationToken);
        }
    }
}
