using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract
{
    public static partial class AbstractRecycleBinHelpers
    {
        public static async Task<long> GetOccupiedSizeAsync(IModifiableFolder recycleBin, CancellationToken cancellationToken = default)
        {
            var recycleBinConfig = await recycleBin.TryGetFileByNameAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, cancellationToken);
            recycleBinConfig ??= await recycleBin.CreateFileAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, false, cancellationToken);

            var text = await recycleBinConfig.ReadAllTextAsync(null, cancellationToken);
            return !long.TryParse(text, out var value) ? 0L : Math.Max(0L, value);
        }

        public static async Task SetOccupiedSizeAsync(IModifiableFolder recycleBin, long value, CancellationToken cancellationToken = default)
        {
            var recycleBinConfig = await recycleBin.TryGetFileByNameAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, cancellationToken);
            recycleBinConfig ??= await recycleBin.CreateFileAsync(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, false, cancellationToken);

            await recycleBinConfig.WriteAllTextAsync(Math.Max(0L, value).ToString(), null, cancellationToken);
        }

        public static async Task<RecycleBinItemDataModel> GetItemDataModelAsync(IStorableChild item, IFolder recycleBin, IAsyncSerializer<Stream> streamSerializer, CancellationToken cancellationToken = default)
        {
            // Get the configuration file
            var configurationFile = !item.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? await recycleBin.GetFileByNameAsync($"{item.Name}.json", cancellationToken)
                : (IFile)item;

            // Read configuration file
            await using var configurationStream = await configurationFile.OpenReadAsync(cancellationToken);

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
