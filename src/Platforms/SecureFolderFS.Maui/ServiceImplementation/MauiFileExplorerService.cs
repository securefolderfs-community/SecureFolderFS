using CommunityToolkit.Maui.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.UI.Storage.NativeStorage;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    internal sealed class MauiFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task OpenAppFolderAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task OpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            var fileSaver = FileSaver.Default;
            var result = await fileSaver.SaveAsync(suggestedName, dataStream, cancellationToken);

            return result.IsSuccessful;
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = FilePicker.Default;
            var result = await filePicker.PickAsync();
            if (result is null)
                return null;

            return new NativeFile(new FileInfo(result.FullPath));
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = FolderPicker.Default;
            var result = await folderPicker.PickAsync(cancellationToken);
            if (!result.IsSuccessful)
                return null;

            return new NativeFolder(new DirectoryInfo(result.Folder.Path));
        }
    }
}
