using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.AppModels;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IRecycleBinService"/>
    public class RecycleBinService : IRecycleBinService
    {
        /// <inheritdoc/>
        public async Task ConfigureRecycleBinAsync(IVFSRoot vfsRoot, long maxSize, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported.");
            
            if (specificsWrapper.Inner.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            // TODO: Update configuration data model with appropriate IsRecycleBinEnabled value
            vfsRoot.Options.DangerousSetRecycleBin(maxSize);

            await Task.CompletedTask;
        }
        
        /// <inheritdoc/>
        public async Task RecalculateSizesAsync(IVFSRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported.");

            if (specificsWrapper.Inner.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;
            
            var recycleBin = await AbstractRecycleBinHelpers.TryGetRecycleBinAsync(specificsWrapper.Inner, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                return;

            var totalSize = 0L;
            await foreach (var item in recycleBin.GetItemsAsync(StorableType.File, cancellationToken))
            {
                if (item.Name.EndsWith(".json"))
                    continue;
                
                var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(item, recycleBin, StreamSerializer.Instance, cancellationToken);
                if (dataModel.Size is { } size and >= 0L)
                {
                    totalSize += size;
                    continue;
                }
                
                // Get the configuration file
                var configurationFile = await recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken);

                // Read configuration file
                await using var configurationStream = await configurationFile.OpenReadAsync(cancellationToken);
                
                // Calculate new size
                var sizeHint = item switch
                {
                    IFile file => await file.GetSizeAsync(cancellationToken),
                    IFolder folder => await folder.GetSizeAsync(cancellationToken),
                    _ => 0L
                };
                totalSize += sizeHint;
                
                // Create new configuration with updated size
                var newConfigurationDataModel = dataModel with
                {
                    Size = sizeHint
                };
                
                // Serialize configuration data model
                await using var serializedStream = await StreamSerializer.Instance.SerializeAsync(newConfigurationDataModel, cancellationToken);

                // Write to destination stream
                await serializedStream.CopyToAsync(configurationStream, cancellationToken);
                await configurationStream.FlushAsync(cancellationToken);
            }
            
            await AbstractRecycleBinHelpers.SetOccupiedSizeAsync(modifiableRecycleBin, totalSize, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IRecycleBinFolder> GetRecycleBinAsync(IVFSRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported.");

            var recycleBin = await AbstractRecycleBinHelpers.TryGetRecycleBinAsync(specificsWrapper.Inner, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("Could not retrieve the recycle bin folder.");

            return new VaultRecycleBin(modifiableRecycleBin, vfsRoot, specificsWrapper.Inner, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public async Task<IRecycleBinFolder> GetOrCreateRecycleBinAsync(IVFSRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported.");

            var recycleBin = await AbstractRecycleBinHelpers.GetOrCreateRecycleBinAsync(specificsWrapper.Inner, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("Could not retrieve the recycle bin folder.");

            return new VaultRecycleBin(modifiableRecycleBin, vfsRoot, specificsWrapper.Inner, StreamSerializer.Instance);
        }
    }
}
