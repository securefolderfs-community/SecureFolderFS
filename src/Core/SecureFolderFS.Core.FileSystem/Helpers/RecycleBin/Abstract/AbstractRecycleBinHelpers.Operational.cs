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
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static partial class AbstractRecycleBinHelpers
    {
        /// <summary>
        /// The threshold in seconds for detecting recently created files.
        /// Files created within this threshold will be deleted immediately instead of recycled.
        /// This helps work around macOS Finder behavior during copy operations.
        /// </summary>
        private const int RECENT_FILE_THRESHOLD_MS = 3000;

        public static async Task<IModifiableFolder?> GetDestinationFolderAsync(IStorableChild recycleBinItem, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Get recycle bin
            var recycleBin = await TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is null)
                throw new DirectoryNotFoundException("Could not find recycle bin folder.");

            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(recycleBinItem, recycleBin, streamSerializer, cancellationToken);
            if (deserialized is not { ParentId: not null, Name: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            // Get the plaintext name and parent ID
            var plaintextParentPath = deserialized.DecryptParentId(specifics.Security);
            var plaintextOriginalName = deserialized.DecryptName(specifics.Security);
            if (plaintextOriginalName is null || plaintextParentPath is null)
                return null;

            try
            {
                var ciphertextParent = await AbstractPathHelpers.GetCiphertextItemAsync(plaintextParentPath, specifics, cancellationToken);
                return ciphertextParent as IModifiableFolder;
            }
            catch (Exception)
            {
                // Destination folder does not exist, user must choose a new location
                return null;
            }
        }

        public static async Task RestoreAsync(IStorableChild recycleBinItem, IModifiableFolder ciphertextDestinationFolder, FileSystemSpecifics specifics, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // Get recycle bin
            var recycleBin = await TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not modifiable.");

            // Deserialize configuration
            var deserialized = await GetItemDataModelAsync(recycleBinItem, recycleBin, streamSerializer, cancellationToken);
            if (deserialized is not { ParentId: not null, Name: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            // Get the plaintext name and parent ID
            var plaintextParentPath = deserialized.DecryptParentId(specifics.Security);
            var plaintextOriginalName = deserialized.DecryptName(specifics.Security);
            if (plaintextOriginalName is null || plaintextParentPath is null)
                throw new FormatException("Could not decrypt recycle bin configuration file.");

            // Encrypt the name for discovery (no sidecar side-effects)
            var ciphertextName = await AbstractPathHelpers.EncryptNameForDiscoveryAsync(plaintextOriginalName, ciphertextDestinationFolder, specifics, cancellationToken);

            // Get an available name if the destination already has an item with that name
            var (_, finalPlaintextName) = await GetAvailableDestinationNameAsync(ciphertextDestinationFolder, ciphertextName, plaintextOriginalName, specifics, cancellationToken);

            // Materialize the name: writes the sidecar file if the name is shortened
            ciphertextName = await AbstractPathHelpers.EncryptNameForUseAsync(finalPlaintextName, ciphertextDestinationFolder, specifics, cancellationToken);

            // Rename and move item to destination
            _ = await ciphertextDestinationFolder.MoveStorableFromAsync(recycleBinItem, modifiableRecycleBin, false, ciphertextName, null, cancellationToken);

            // Delete the old configuration file
            var configurationFile = await recycleBin.GetFileByNameAsync($"{recycleBinItem.Name}.json", cancellationToken);
            await modifiableRecycleBin.DeleteAsync(configurationFile, cancellationToken);

            // Check if the item had any size
            if (deserialized.Size is not ({ } size and > 0L))
                return;

            // Update occupied size
            var occupiedSize = await GetOccupiedSizeAsync(modifiableRecycleBin, cancellationToken);
            var newSize = occupiedSize - size;
            await SetOccupiedSizeAsync(modifiableRecycleBin, newSize, cancellationToken);
        }

        public static async Task DeleteOrRecycleAsync(
            IModifiableFolder ciphertextSourceFolder,
            IStorableChild ciphertextItem,
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
                await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                return;
            }

            // Allocate Directory ID for later use
            var directoryId = AbstractPathHelpers.AllocateDirectoryId(specifics.Security, ciphertextSourceFolder.Id);

            // Decrypt the plaintext name
            var plaintextName = await AbstractPathHelpers.DecryptNameAsync(ciphertextItem.Name, ciphertextSourceFolder, specifics, directoryId, cancellationToken);
            if (plaintextName is null)
                throw new FormatException("Could not decrypt name for recycle bin configuration file.");

            // Check for wildcard file names
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            {
                if (plaintextName == ".DS_Store" || plaintextName.StartsWith("._", StringComparison.Ordinal))
                {
                    // .DS_Store and Apple Double files are unsupported by the recycle bin, delete immediately
                    await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                    return;
                }

                // Check if the file was recently created (likely part of a copy operation)
                // On macOS, Finder creates files and immediately deletes them during copy operations
                if (await IsRecentlyCreatedAsync(ciphertextItem, cancellationToken))
                {
                    await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                    return;
                }
            }

            // Get recycle bin
            var recycleBin = await GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("The recycle bin is not modifiable.");

            if (sizeHint < 0L && specifics.Options.RecycleBinSize > 0L)
            {
                sizeHint = ciphertextItem switch
                {
                    IFile file => await file.GetSizeAsync(cancellationToken) ?? 0L,
                    IFolder folder => await folder.GetSizeAsync(cancellationToken) ?? 0L,
                    _ => 0L
                };

                var occupiedSize = await GetOccupiedSizeAsync(modifiableRecycleBin, cancellationToken);
                var availableSize = specifics.Options.RecycleBinSize - occupiedSize;
                if (availableSize < sizeHint)
                {
                    await DeleteImmediatelyAsync(ciphertextSourceFolder, ciphertextItem, cancellationToken);
                    return;
                }
            }

            // Rename and move item
            var guid = Guid.NewGuid().ToString();
            _ = await modifiableRecycleBin.MoveStorableFromAsync(ciphertextItem, ciphertextSourceFolder, false, guid, null, cancellationToken);

            // Create configuration file
            var configurationFile = await modifiableRecycleBin.CreateFileAsync($"{guid}.json", false, cancellationToken);
            await using (var configurationStream = await configurationFile.OpenWriteAsync(cancellationToken))
            {
                // Decrypt the plaintext parent ID
                var plaintextParentId = await AbstractPathHelpers.GetPlaintextPathAsync((IStorableChild)ciphertextSourceFolder, specifics, cancellationToken);
                if (plaintextParentId is null)
                    throw new FormatException("Could not decrypt parent path for the recycle bin configuration file.");

                // Determine if Directory ID is present
                var isDirectoryIdPresent = ciphertextSourceFolder.Id != specifics.ContentFolder.Id;

                // Encrypt the new plaintext name and parent ID
                var newCiphertextName = RecycleBinItemDataModel.Encrypt(plaintextName, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);
                var newCiphertextParentId = RecycleBinItemDataModel.Encrypt(plaintextParentId, specifics.Security, isDirectoryIdPresent ? directoryId : ReadOnlySpan<byte>.Empty);

                // Serialize configuration data model
                await using var serializedStream = await streamSerializer.SerializeAsync(
                    new RecycleBinItemDataModel()
                    {
                        Name = newCiphertextName,
                        ParentId = newCiphertextParentId,
                        DirectoryId = isDirectoryIdPresent ? directoryId : [],
                        DeletionTimestamp = DateTime.Now,
                        Size = sizeHint
                    }, cancellationToken);

                // Write to destination stream
                await serializedStream.CopyToAsync(configurationStream, cancellationToken);
                await configurationStream.FlushAsync(cancellationToken);
            }

            // Update occupied size
            if (specifics.Options.IsRecycleBinEnabled())
            {
                var occupiedSize = await GetOccupiedSizeAsync(modifiableRecycleBin, cancellationToken);
                var newSize = occupiedSize + sizeHint;
                await SetOccupiedSizeAsync(modifiableRecycleBin, newSize, cancellationToken);
            }

            static async Task DeleteImmediatelyAsync(IModifiableFolder ciphertextSourceFolder, IStorableChild ciphertextItem, CancellationToken cancellationToken)
            {
                await ciphertextSourceFolder.DeleteAsync(ciphertextItem, deleteImmediately: true, cancellationToken: cancellationToken);
            }
        }

        private static async Task<(string CiphertextName, string PlaintextName)> GetAvailableDestinationNameAsync(IFolder ciphertextDestinationFolder, string ciphertextName, string plaintextOriginalName, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            var finalPlaintextName = plaintextOriginalName;

            // Check if the item already exists
            var existing = await ciphertextDestinationFolder.TryGetFirstByNameAsync(ciphertextName, cancellationToken);
            if (existing is not null)
            {
                // If the item already exists, append a suffix to the name
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(plaintextOriginalName);
                var extension = Path.GetExtension(plaintextOriginalName);
                var suffix = 1;
                do
                {
                    finalPlaintextName = $"{nameWithoutExtension} ({suffix}){extension}";
                    ciphertextName = await AbstractPathHelpers.EncryptNameForDiscoveryAsync(finalPlaintextName, ciphertextDestinationFolder, specifics, cancellationToken);
                    existing = await ciphertextDestinationFolder.TryGetFirstByNameAsync(ciphertextName, cancellationToken);
                    suffix++;
                } while (existing is not null);
            }

            return (ciphertextName, finalPlaintextName);
        }

        private static async Task<bool> IsRecentlyCreatedAsync(IStorable storable, CancellationToken cancellationToken)
        {
            try
            {
                var dateCreated = await storable.GetDateCreatedAsync(cancellationToken);
                if (dateCreated is null)
                    return false;

                var dateCreatedUtc = dateCreated.Value.ToUniversalTime();
                var timeSinceCreation = DateTime.UtcNow - dateCreatedUtc;
                return timeSinceCreation.Seconds <= RECENT_FILE_THRESHOLD_MS / 1000;
            }
            catch
            {
                // If we can't determine creation time, assume it's not recent
                return false;
            }
        }
    }
}
