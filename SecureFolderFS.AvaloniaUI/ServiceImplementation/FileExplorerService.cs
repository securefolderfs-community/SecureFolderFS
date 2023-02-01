using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Platform.Storage;
using SecureFolderFS.AvaloniaUI.Helpers;
using SecureFolderFS.AvaloniaUI.WindowViews;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Storage.NativeStorage;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class FileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task OpenAppFolderAsync(CancellationToken cancellationToken = default)
        {
            LauncherHelper.Launch(App.AppDirectory);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OpenInFileExplorerAsync(ILocatableFolder folder, CancellationToken cancellationToken = default)
        {
            LauncherHelper.Launch(folder.Path);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> PickSingleFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var file = await MainWindow.Instance.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter = filter is null
                    ? new List<FilePickerFileType>(new[] { new FilePickerFileType("*") }).AsReadOnly()
                    : filter.Select(x => new FilePickerFileType(x)).ToList().AsReadOnly()
            });

            if (!file.First().TryGetUri(out var uri))
                return null;

            return new NativeFile(HttpUtility.UrlDecode(uri.AbsolutePath));
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder?> PickSingleFolderAsync(CancellationToken cancellationToken = default)
        {
            var folder = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(new()
            {
                AllowMultiple = false
            });

            if (folder.IsEmpty())
                return null;

            if (!folder.First().TryGetUri(out var uri))
                return null;

            return new NativeFolder(HttpUtility.UrlDecode(uri.AbsolutePath));
        }
    }
}