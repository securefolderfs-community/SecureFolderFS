using Avalonia.Platform.Storage;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.AvaloniaUI.WindowViews;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Storage.NativeStorage;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class FileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task OpenAppFolderAsync(CancellationToken cancellationToken = default)
        {
            LauncherHelpers.Launch(App.AppDirectory);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is not ILocatableFolder locatableFolder)
                return Task.CompletedTask;

            LauncherHelpers.Launch(locatableFolder.Path);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var file = (await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter = filter is null
                    ? new List<FilePickerFileType>(new[] { new FilePickerFileType("*") }).AsReadOnly()
                    : filter.Select(x => new FilePickerFileType(x)).ToList().AsReadOnly()
            })).FirstOrDefault();

            if (file is null)
                return null;

            return new NativeFile(HttpUtility.UrlDecode(file.Path.AbsolutePath));
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folder = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(new()
            {
                AllowMultiple = false
            });

            if (folder.IsEmpty())
                return null;

            var firstFolder = folder.FirstOrDefault();
            if (firstFolder is null)
                return null;

            return new NativeFolder(HttpUtility.UrlDecode(firstFolder.Path.AbsolutePath));
        }
    }
}