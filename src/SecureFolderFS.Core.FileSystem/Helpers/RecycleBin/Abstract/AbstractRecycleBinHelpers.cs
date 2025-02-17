using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static class AbstractRecycleBinHelpers
    {
        public static async Task<RecycleBinItemDataModel> GetItemDataModelAsync(IStorableChild item, IFolder recycleBin, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Read configuration file
            var configurationFile = await recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken);
            await using var configurationStream = await configurationFile.OpenReadAsync(cancellationToken);

            // Deserialize configuration
            var deserialized = await streamSerializer.DeserializeAsync<Stream, RecycleBinItemDataModel>(configurationStream, cancellationToken);
            if (deserialized is not { OriginalPath: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            return deserialized;
        }
        
        public static async Task<IModifiableFolder?> GetDestinationFolderAsync(IStorableChild item, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            
            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(item, recycleBin, streamSerializer, cancellationToken);
            if (deserialized is not { OriginalPath: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");
            
            // Check if destination item exists
            var id = deserialized.OriginalPath.Replace('/', Path.DirectorySeparatorChar);
            try
            {
                _ = await recycleBin.GetItemByRelativePathAsync(id, cancellationToken);
                
                // Destination item already exists, user must choose a new location
                return null;
            }
            catch (Exception) { }

            // Check if destination folder exists
            var parentId = Path.GetDirectoryName(id) ?? string.Empty;
            try
            {
                var parentItem = await recycleBin.GetItemByRelativePathAsync(parentId, cancellationToken);
                
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
                throw new UnauthorizedAccessException("The vault is read-only.");
                
            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IRenamableFolder renamableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not renamable.");
            
            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(item, recycleBin, streamSerializer, cancellationToken);
           
            // Rename the item to correct name
            var originalName = Path.GetFileName(deserialized.OriginalPath) ?? throw new IOException("Could not get file name.");
            var renamedItem = await renamableRecycleBin.RenameAsync(item, originalName, cancellationToken);

            // Move item to destination
            _ = await destinationFolder.MoveStorableFromAsync(renamedItem, renamableRecycleBin, false, cancellationToken);
        }
        
        public static async Task DeleteOrTrashAsync(IModifiableFolder sourceFolder, IStorableChild item, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                throw new UnauthorizedAccessException("The vault is read-only.");

            if (!specifics.Options.IsRecycleBinEnabled)
            {
                await sourceFolder.DeleteAsync(item, cancellationToken);
                return;
            }
            
            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IRenamableFolder renamableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not renamable.");
            
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
                    OriginalPath = item.Id.Replace(Path.DirectorySeparatorChar, '/'),
                    DeletionTimestamp = DateTime.Now
                }, cancellationToken);
            
            // Write to destination stream
            await serializedStream.CopyToAsync(configurationStream, cancellationToken);
        }
        
        public static async Task<IFolder> GetOrCreateRecycleBinAsync(FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            try
            {
                return await specifics.ContentFolder.GetFolderByNameAsync(Constants.Names.RECYCLE_BIN_NAME, cancellationToken);
            }
            catch (Exception) { }
            
            if (specifics.ContentFolder is not IModifiableFolder modifiableFolder)
                throw new UnauthorizedAccessException("The content folder is not modifiable.");

            return await modifiableFolder.CreateFolderAsync(Constants.Names.RECYCLE_BIN_NAME, false, cancellationToken);
        }
    }
}
