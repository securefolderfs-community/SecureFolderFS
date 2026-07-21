using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Storage;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IRecycleBinService"/>
    public class RecycleBinService : IRecycleBinService
    {
        /// <inheritdoc/>
        public async Task ConfigureRecycleBinAsync(UnlockedVaultViewModel unlockedViewModel, long maxSize, CancellationToken cancellationToken = default)
        {
            if (unlockedViewModel.StorageRoot is not IWrapper<FileSystemSpecifics> { Inner: { } specifics })
                throw new ArgumentException($"The specified {nameof(IVfsRoot)} instance is not supported.");

            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            var vaultReader = new VaultReader(unlockedViewModel.VaultFolder, StreamSerializer.Instance);
            var vaultWriter = new VaultWriter(unlockedViewModel.VaultFolder, StreamSerializer.Instance);

            // Read configuration
            var configDataModel = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var newConfigDataModel = configDataModel with
            {
                RecycleBinSize = maxSize,
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };

            // First, we need to fill in the PayloadMac of the content
            specifics.Security.KeyPair.MacKey.UseKey(macKey =>
            {
                VaultParser.CalculateConfigMac(newConfigDataModel, macKey, newConfigDataModel.PayloadMac);
            });

            // Then, write the config
            await vaultWriter.WriteConfigurationAsync(newConfigDataModel, cancellationToken);

            // Make sure to also update the file system options
            specifics.Options.DangerousSetRecycleBin(maxSize);
        }

        /// <inheritdoc/>
        public async Task RecalculateSizesAsync(IVfsRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> { Inner: { } specifics })
                throw new ArgumentException($"The specified {nameof(IVfsRoot)} instance is not supported.");

            if (specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            var recycleBin = await AbstractRecycleBinHelpers.TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                return;

            // Split the recycle bin contents into payloads and their configuration files
            var payloadItems = new List<IStorableChild>();
            var configurationFiles = new Dictionary<string, IChildFile>(StringComparer.OrdinalIgnoreCase);
            await foreach (var item in recycleBin.GetItemsAsync(StorableType.All, cancellationToken))
            {
                if (PathHelpers.IsCoreName(item.Name))
                    continue;

                if (item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    if (item is IChildFile configurationFile)
                        configurationFiles[Path.GetFileNameWithoutExtension(item.Name)] = configurationFile;

                    continue;
                }

                payloadItems.Add(item);
            }

            var totalSize = 0L;
            foreach (var payloadItem in payloadItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Payloads without a configuration file cannot be restored, but they still
                // occupy space and are surfaced in the recycle bin view for manual deletion
                if (!configurationFiles.Remove(payloadItem.Name, out var configurationFile))
                {
                    totalSize += await SafetyHelpers.NoFailureAsync(async () => await AbstractRecycleBinHelpers.GetPlaintextSizeAsync(payloadItem, specifics, cancellationToken));
                    continue;
                }

                // A single corrupt entry must not abandon the recalculation of the remaining ones
                try
                {
                    var dataModel = await AbstractRecycleBinHelpers.GetItemDataModelAsync(configurationFile, recycleBin, StreamSerializer.Instance, cancellationToken);
                    if (dataModel.Size is { } size and >= 0L)
                    {
                        totalSize += size;
                        continue;
                    }

                    // Calculate new size
                    var sizeHint = await AbstractRecycleBinHelpers.GetPlaintextSizeAsync(payloadItem, specifics, cancellationToken);
                    totalSize += sizeHint;

                    // Create new configuration with updated size
                    var newConfigurationDataModel = dataModel with
                    {
                        Size = sizeHint
                    };

                    // Rewrite the configuration file, truncating any previous content
                    await using var configurationStream = await configurationFile.OpenWriteAsync(cancellationToken);
                    if (configurationStream.CanSeek)
                        configurationStream.SetLength(0L);

                    await using var serializedStream = await StreamSerializer.Instance.SerializeAsync(newConfigurationDataModel, cancellationToken);
                    await serializedStream.CopyToAsync(configurationStream, cancellationToken);
                    await configurationStream.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    totalSize += await SafetyHelpers.NoFailureAsync(async () => await AbstractRecycleBinHelpers.GetPlaintextSizeAsync(payloadItem, specifics, cancellationToken));
                }
            }

            // Any remaining configuration files have no payload (e.g., after an interrupted
            // recycle operation) and are safe to remove
            foreach (var orphanedConfiguration in configurationFiles.Values)
                await SafetyHelpers.NoFailureAsync(async () => await modifiableRecycleBin.DeleteAsync(orphanedConfiguration, cancellationToken));

            await specifics.RecycleBinSemaphore.WaitAsync(cancellationToken);
            try
            {
                await AbstractRecycleBinHelpers.SetOccupiedSizeAsync(modifiableRecycleBin, totalSize, cancellationToken);
            }
            finally
            {
                _ = specifics.RecycleBinSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<IRecycleBinFolder> GetRecycleBinAsync(IVfsRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> { Inner: { } specifics })
                throw new ArgumentException($"The specified {nameof(IVfsRoot)} instance is not supported.");

            var recycleBin = await AbstractRecycleBinHelpers.TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("Could not retrieve the recycle bin folder.");

            return new RecycleBinFolder(modifiableRecycleBin, vfsRoot, specifics, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public async Task<IRecycleBinFolder> GetOrCreateRecycleBinAsync(IVfsRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> { Inner: { } specifics })
                throw new ArgumentException($"The specified {nameof(IVfsRoot)} instance is not supported.");

            var recycleBin = await AbstractRecycleBinHelpers.GetOrCreateRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("Could not retrieve the recycle bin folder.");

            return new RecycleBinFolder(modifiableRecycleBin, vfsRoot, specifics, StreamSerializer.Instance);
        }
    }
}
