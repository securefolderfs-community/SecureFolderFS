using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Abstract;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
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
        public async IAsyncEnumerable<RecycleBinItemViewModel> GetRecycleBinItemsAsync(IVFSRoot vfsRoot, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new NotSupportedException($"The specified {nameof(IVFSRoot)} instance is not supported.");

            var specifics = specificsWrapper.Inner;
            var recycleBinFolder = await AbstractRecycleBinHelpers.GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            await foreach (var item in recycleBinFolder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, recycleBinFolder, StreamSerializer.Instance, cancellationToken);
                if (dataModel.ParentPath is null || dataModel.OriginalName is null)
                    continue;
                
                var parentFolder = await specifics.ContentFolder.GetItemByRelativePathOrSelfAsync(dataModel.ParentPath, cancellationToken) as IFolder;
                if (parentFolder is null)
                    continue;
                    
                var plaintextName = await AbstractPathHelpers.DecryptNameAsync(dataModel.OriginalName, parentFolder, specifics, cancellationToken);
                yield return new(item)
                {
                    Title = plaintextName,
                    DeletionTimestamp = dataModel.DeletionTimestamp
                };
            }
        }

        /// <inheritdoc/>
        public async Task RestoreItemAsync(IVFSRoot vfsRoot, IStorableChild recycleBinItem, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                return;

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
                    return;

                if (!destinationFolder.Id.Contains(vfsRoot.VirtualizedRoot.Id, StringComparison.OrdinalIgnoreCase))
                {
                    // Invalid folder chosen outside of vault
                    // TODO: Return IResult or throw
                    return;
                }
            }

            // Restore the item to chosen destination
            await AbstractRecycleBinHelpers.RestoreAsync(
                recycleBinItem,
                destinationFolder,
                specifics,
                StreamSerializer.Instance,
                cancellationToken);
        }
    }
}
