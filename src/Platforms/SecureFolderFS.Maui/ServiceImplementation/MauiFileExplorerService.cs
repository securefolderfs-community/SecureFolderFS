using CommunityToolkit.Maui.Storage;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    internal sealed class MauiFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            // TODO: Try to implement opening in mobile file explorer
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            var fileSaver = FileSaver.Default;
            var result = await fileSaver.SaveAsync(suggestedName, dataStream, cancellationToken);

            return result.IsSuccessful;
        }

        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = FilePicker.Default;
            var result = await filePicker.PickAsync();
            if (result is null)
                return null;

            return new SystemFile(new FileInfo(result.FullPath));
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = FolderPicker.Default;
            var result = await folderPicker.PickAsync(cancellationToken);
            if (!result.IsSuccessful)
                return null;

            return new SystemFolder(new DirectoryInfo(result.Folder.Path));
        }
    }
}