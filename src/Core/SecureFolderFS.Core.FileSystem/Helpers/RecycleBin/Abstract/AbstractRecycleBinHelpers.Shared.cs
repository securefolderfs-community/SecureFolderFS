using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Core.FileSystem.Helpers.Paths;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static partial class AbstractRecycleBinHelpers
    {
        public static async Task<long> GetOccupiedSizeAsync(IModifiableFolder recycleBin, CancellationToken cancellationToken = default)
        {
            // Reading must not create the configuration file - reads can happen on read-only file systems
            var recycleBinConfig = await recycleBin.TryGetFileByNameAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, cancellationToken);
            if (recycleBinConfig is null)
                return 0L;

            await using var configStream = await recycleBinConfig.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            var deserialized = await StreamSerializer.Instance.TryDeserializeAsync<Stream, RecycleBinDataModel>(configStream, cancellationToken);
            if (deserialized is null)
                return 0L;

            return Math.Max(0L, deserialized.OccupiedSize);
        }

        public static async Task SetOccupiedSizeAsync(IModifiableFolder recycleBin, long value, CancellationToken cancellationToken = default)
        {
            // Recreate the file instead of opening it for write - streams returned by
            // OpenWriteAsync do not truncate, which leaves stale tail bytes (and therefore
            // unparseable JSON) whenever the serialized payload shrinks
            var recycleBinConfig = await recycleBin.CreateFileAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, true, cancellationToken);

            await using var configStream = await recycleBinConfig.OpenWriteAsync(cancellationToken);
            if (configStream.CanSeek)
                configStream.SetLength(0L);

            await using var serialized = await StreamSerializer.Instance.SerializeAsync(new RecycleBinDataModel()
            {
                OccupiedSize = Math.Max(0L, value)
            }, cancellationToken);

            await serialized.CopyToAsync(configStream, cancellationToken);
            await configStream.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Atomically adds <paramref name="delta"/> to the occupied size of the recycle bin.
        /// </summary>
        public static async Task AdjustOccupiedSizeAsync(IModifiableFolder recycleBin, FileSystemSpecifics specifics, long delta, CancellationToken cancellationToken = default)
        {
            await specifics.RecycleBinSemaphore.WaitAsync(cancellationToken);
            try
            {
                var occupiedSize = await GetOccupiedSizeAsync(recycleBin, cancellationToken);
                await SetOccupiedSizeAsync(recycleBin, occupiedSize + delta, cancellationToken);
            }
            finally
            {
                _ = specifics.RecycleBinSemaphore.Release();
            }
        }

        /// <summary>
        /// Measures the plaintext size in bytes of a ciphertext item. Folder sizes are walked iteratively.
        /// </summary>
        public static async Task<long> GetPlaintextSizeAsync(IStorableChild ciphertextItem, FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            return ciphertextItem switch
            {
                IFile file => CalculatePlaintextSize(await GetCiphertextFileSizeAsync(file, cancellationToken), specifics),
                IFolder folder => await GetFolderPlaintextSizeAsync(folder, specifics, cancellationToken),
                _ => 0L
            };
        }

        private static async Task<long> GetCiphertextFileSizeAsync(IFile ciphertextFile, CancellationToken cancellationToken)
        {
            // Raw ciphertext files usually don't implement ISizeOf - fall back to the stream length
            var size = await ciphertextFile.GetSizeAsync(cancellationToken);
            if (size is not null)
                return size.Value;

            try
            {
                await using var stream = await ciphertextFile.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
                return stream.Length;
            }
            catch (Exception)
            {
                return 0L;
            }
        }

        private static long CalculatePlaintextSize(long ciphertextLength, FileSystemSpecifics specifics)
        {
            return Math.Max(0L, specifics.Security.ContentCrypt.CalculatePlaintextSize(
                Math.Max(0L, ciphertextLength - specifics.Security.HeaderCrypt.HeaderCiphertextSize)));
        }

        private static async Task<long> GetFolderPlaintextSizeAsync(IFolder ciphertextFolder, FileSystemSpecifics specifics, CancellationToken cancellationToken)
        {
            var totalSize = 0L;
            var stack = new Stack<IFolder>();
            stack.Push(ciphertextFolder);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                await foreach (var item in current.GetItemsAsync(StorableType.All, cancellationToken))
                {
                    switch (item)
                    {
                        case IFile file when !PathHelpers.IsCoreName(file.Name):
                            totalSize += CalculatePlaintextSize(await GetCiphertextFileSizeAsync(file, cancellationToken), specifics);
                            break;

                        case IFolder subFolder:
                            stack.Push(subFolder);
                            break;
                    }
                }
            }

            return totalSize;
        }

        public static async Task<RecycleBinItemDataModel> GetItemDataModelAsync(IStorableChild item, IFolder recycleBin, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Get the configuration file
            var configurationFile = !item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? await recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken)
                : (IFile)item;

            // Read configuration file
            await using var configurationStream = await configurationFile.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);

            // Deserialize configuration
            var deserialized = await streamSerializer.DeserializeAsync<Stream, RecycleBinItemDataModel>(configurationStream, cancellationToken);
            if (deserialized is not { ParentId: not null })
                throw new FormatException("Could not deserialize recycle bin configuration file.");

            return deserialized;
        }

        public static async Task<IFolder> GetOrCreateRecycleBinAsync(FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            var recycleBin = await TryGetRecycleBinAsync(specifics, cancellationToken);
            if (recycleBin is not null)
                return recycleBin;

            if (specifics.ContentFolder is not IModifiableFolder modifiableFolder || specifics.Options.IsReadOnly)
                throw FileSystemExceptions.FileSystemReadOnly;

            return await modifiableFolder.CreateFolderAsync(Constants.Names.RECYCLE_BIN_NAME, false, cancellationToken);
        }

        public static async Task<IFolder?> TryGetRecycleBinAsync(FileSystemSpecifics specifics, CancellationToken cancellationToken = default)
        {
            return await specifics.ContentFolder.TryGetFolderByNameAsync(Constants.Names.RECYCLE_BIN_NAME, cancellationToken);
        }
    }
}
