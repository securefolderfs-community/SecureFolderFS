using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static partial class AbstractRecycleBinHelpers
    {
        public static async Task<IModifiableFolder?> GetDestinationFolderAsync(IStorableChild item, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Get recycle bin
            var recycleBin = await TryGetRecycleBinAsync(specifics, cancellationToken);
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
            var recycleBin = await TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IRenamableFolder renamableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not renamable.");

            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(item, recycleBin, streamSerializer, cancellationToken);
            _ = deserialized.OriginalName ?? throw new IOException("Could not get file name.");
            _ = deserialized.ParentPath ?? throw new IOException("Could not get parent path.");

            if (!destinationFolder.Id.EndsWith(deserialized.ParentPath))
            {
                // Destination folder is different from the original destination
                // A new item name should be chosen fit for the new folder (so that Directory ID match)

                var plaintextName = specifics.Security.NameCrypt?.DecryptName(Path.GetFileNameWithoutExtension(deserialized.OriginalName), deserialized.DirectoryId) ?? deserialized.OriginalName;
                var ciphertextName = await AbstractPathHelpers.EncryptNameAsync(plaintextName, destinationFolder, specifics, cancellationToken);

                // Rename the item to correct name
                var renamedItem = await renamableRecycleBin.RenameAsync(item, ciphertextName, cancellationToken);

                // Move item to destination
                _ = await destinationFolder.MoveStorableFromAsync(renamedItem, renamableRecycleBin, false, cancellationToken);
            }
            else
            {
                // Destination folder is the same as the original destination
                // The same name could be used since the Directory IDs match
                // TODO: Check if the Directory IDs actually match and fallback to above method if not

                // Rename the item to correct name
                var renamedItem = await renamableRecycleBin.RenameAsync(item, deserialized.OriginalName, cancellationToken);

                // Move item to destination
                _ = await destinationFolder.MoveStorableFromAsync(renamedItem, renamableRecycleBin, false, cancellationToken);
            }

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

        /// <summary>
        /// The threshold in seconds for detecting recently created files.
        /// Files created within this threshold will be deleted immediately instead of recycled.
        /// This helps work around macOS Finder behavior during copy operations.
        /// </summary>
        private const double RECENT_FILE_THRESHOLD_SECONDS = 5.0;

        public static async Task DeleteOrRecycleAsync(
            IModifiableFolder sourceFolder,
            IStorableChild item,
            FileSystemSpecifics specifics,
            IAsyncSerializer<Stream> streamSerializer,
            long sizeHint = -1L,
            bool deleteImmediately = false,
            CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            if (!specifics.Options.IsRecycleBinEnabled() || deleteImmediately)
            {
                await sourceFolder.DeleteAsync(item, cancellationToken);
                return;
            }

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                var parentFolder = await item.GetParentAsync(cancellationToken);
                if (parentFolder is not null)
                {
                    var plaintextName = await AbstractPathHelpers.DecryptNameAsync(item.Name, parentFolder, specifics, cancellationToken) ?? string.Empty;
                    if (plaintextName == ".DS_Store" || plaintextName.StartsWith("._", StringComparison.Ordinal))
                    {
                        // .DS_Store and Apple Double files are not supported by the recycle bin, delete immediately
                        await sourceFolder.DeleteAsync(item, cancellationToken);
                        return;
                    }
                }

                // Check if the file was recently created (likely part of a copy operation)
                // On macOS, Finder creates files and immediately deletes them during copy operations
                if (await IsRecentlyCreatedAsync(item, cancellationToken))
                {
                    await sourceFolder.DeleteAsync(item, cancellationToken);
                    return;
                }
            }

            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IRenamableFolder renamableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not renamable.");

            if (sizeHint < 0L && specifics.Options.RecycleBinSize > 0L)
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
            await using var configurationStream = await configurationFile.OpenWriteAsync(cancellationToken);

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
            await configurationStream.FlushAsync(cancellationToken);

            // Update occupied size
            if (specifics.Options.IsRecycleBinEnabled())
            {
                var occupiedSize = await GetOccupiedSizeAsync(renamableRecycleBin, cancellationToken);
                var newSize = occupiedSize + sizeHint;
                await SetOccupiedSizeAsync(renamableRecycleBin, newSize, cancellationToken);
            }
        }

        private static async Task<bool> IsRecentlyCreatedAsync(IStorable storable, CancellationToken cancellationToken)
        {
            try
            {
                var dateCreated = await storable.GetDateCreatedAsync(cancellationToken);
                if (dateCreated == DateTime.MinValue)
                    return false;

                var timeSinceCreation = DateTime.UtcNow - dateCreated.ToUniversalTime();
                return timeSinceCreation.TotalSeconds <= RECENT_FILE_THRESHOLD_SECONDS;
            }
            catch
            {
                // If we can't determine creation time, assume it's not recent
                return false;
            }
        }
    }
}
