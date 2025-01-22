using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static class AbstractRecycleBinHelpers
    {
        public static async Task RestoreAsync(IStorableChild item, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                return;
            
            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            
            // Read configuration file
            var configurationFile = await recycleBin.GetFileByNameAsync(Constants.Names.RECYCLE_BIN_NAME, cancellationToken);
            await using var configurationStream = await configurationFile.OpenReadAsync(cancellationToken);

            // Deserialize configuration
            var deserialized = await streamSerializer.DeserializeAsync<Stream, RecycleBinItemDataModel>(configurationStream, cancellationToken);
            if (deserialized is not { OriginalPath: { }})
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            // Get parent destination folder
            var id = Path.GetDirectoryName(deserialized.OriginalPath.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty;
            var parentFolder = await specifics.ContentFolder.GetItemRecursiveAsync(id, cancellationToken);
            if (parentFolder is not IModifiableFolder modifiableParent)
                throw new UnauthorizedAccessException("The parent folder is not modifiable.");
            
            await RestoreAsync(item, modifiableParent, specifics, cancellationToken);
        }

        public static async Task RestoreAsync(IStorableChild item, IModifiableFolder destinationFolder, FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IRenamableFolder renamableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not renamable.");
        }
        
        public static async Task DeleteOrTrashAsync(IModifiableFolder sourceFolder, IStorableChild item, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                return;
            
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
            catch (Exception ex)
            {
                _ = ex;
            }
            
            if (specifics.ContentFolder is not IModifiableFolder modifiableFolder)
                throw new UnauthorizedAccessException("The content folder is not modifiable.");

            return await modifiableFolder.CreateFolderAsync(Constants.Names.RECYCLE_BIN_NAME, false, cancellationToken);
        }
    }
}
