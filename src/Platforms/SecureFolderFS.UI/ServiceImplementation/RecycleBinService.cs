using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IRecycleBinService"/>
    public class RecycleBinService : IRecycleBinService
    {
        /// <inheritdoc/>
        public async Task<bool> ToggleRecycleBinAsync(IFolder vaultFolder, IVFSRoot vfsRoot, bool value, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                return false;

            // TODO: Update configuration data model with appropriate IsRecycleBinEnabled value

            vfsRoot.Options.DangerousSetRecycleBin(value);
            return true;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<RecycleBinItemModel> GetRecycleBinItemsAsync(IVFSRoot vfsRoot, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported.");

            var specifics = specificsWrapper.Inner;
            var recycleBinFolder = await AbstractRecycleBinHelpers.GetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBinFolder is null)
                yield break;
            
            await foreach (var item in recycleBinFolder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, recycleBinFolder, StreamSerializer.Instance, cancellationToken);
                if (dataModel.ParentPath is null || dataModel.OriginalName is null)
                    continue;
                
                // Decrypt name only when using name encryption
                var plaintextName = specifics.Security.NameCrypt is null
                    ? dataModel.OriginalName
                    : specifics.Security.NameCrypt.DecryptName(Path.GetFileNameWithoutExtension(dataModel.OriginalName), dataModel.DirectoryId);

                yield return new RecycleBinItemModel(plaintextName, item, dataModel.DeletionTimestamp);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> RestoreItemAsync(IVFSRoot vfsRoot, IStorableChild recycleBinItem, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                return Result.Failure(new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported."));

            try
            {
                var specifics = specificsWrapper.Inner;
                var destinationFolder = await AbstractRecycleBinHelpers.GetDestinationFolderAsync(
                    recycleBinItem,
                    specifics,
                    StreamSerializer.Instance,
                    cancellationToken);

                // Prompt the user to pick the folder when the default destination couldn't be used
                if (destinationFolder is null)
                {
                    // TODO: Add starting directory parameter
                    var fileExplorerService = DI.Service<IFileExplorerService>();
                    destinationFolder = await fileExplorerService.PickFolderAsync(false, cancellationToken) as IModifiableFolder;
                    if (destinationFolder is null)
                        return Result.Failure(new OperationCanceledException("The user did not pick destination a folder."));

                    if (!destinationFolder.Id.Contains(vfsRoot.VirtualizedRoot.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        // Invalid folder chosen outside of vault
                        // TODO: Return IResult or throw
                        return Result.Failure(new InvalidOperationException("The folder is outside of the virtualized storage folder."));
                    }
                }
                // Restore the item to chosen destination
                await AbstractRecycleBinHelpers.RestoreAsync(
                    recycleBinItem,
                    destinationFolder,
                    specifics,
                    StreamSerializer.Instance,
                    cancellationToken);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }
        
        /// <inheritdoc/>
        public async Task<IResult> DeletePermanentlyAsync(IStorableChild recycleBinItem, CancellationToken cancellationToken = default)
        {
            try
            {
                var recycleBin = await recycleBinItem.GetParentAsync(cancellationToken);
                if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                    return Result.Failure(new InvalidOperationException("The recycle bin is not modifiable."));

                // Get the associated configuration file
                var configurationFile = await recycleBin.GetFileByNameAsync($"{recycleBinItem.Name}.json", cancellationToken);

                // Delete both items
                await modifiableRecycleBin.DeleteAsync(recycleBinItem, cancellationToken);
                await modifiableRecycleBin.DeleteAsync(configurationFile, cancellationToken);

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }
    }
}
