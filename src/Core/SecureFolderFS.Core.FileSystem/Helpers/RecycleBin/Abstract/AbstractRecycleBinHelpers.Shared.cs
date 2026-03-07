using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
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
            var recycleBinConfig = await recycleBin.TryGetFileByNameAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, cancellationToken);
            recycleBinConfig ??= await recycleBin.CreateFileAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, false, cancellationToken);

            await using var configStream = await recycleBinConfig.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            var deserialized = await StreamSerializer.Instance.TryDeserializeAsync<Stream, RecycleBinDataModel>(configStream, cancellationToken);
            if (deserialized is null)
                return 0L;

            return Math.Max(0L, deserialized.OccupiedSize);
        }

        public static async Task SetOccupiedSizeAsync(IModifiableFolder recycleBin, long value, CancellationToken cancellationToken = default)
        {
            var recycleBinConfig = await recycleBin.TryGetFileByNameAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, cancellationToken);
            recycleBinConfig ??= await recycleBin.CreateFileAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, false, cancellationToken);

            await using var configStream = await recycleBinConfig.OpenWriteAsync(cancellationToken);
            await using var serialized = await StreamSerializer.Instance.SerializeAsync(new RecycleBinDataModel()
            {
                OccupiedSize = Math.Max(0L, value)
            }, cancellationToken);

            await serialized.CopyToAsync(configStream, cancellationToken);
            await configStream.FlushAsync(cancellationToken);
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
            if (deserialized is not { ParentPath: not null })
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
