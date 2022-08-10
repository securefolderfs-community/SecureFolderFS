using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.WinUI.Storage.WindowsStorage;
using SecureFolderFS.WinUI.WindowViews;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class FileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task OpenAppFolderAsync(CancellationToken cancellationToken = default)
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task OpenInFileExplorerAsync(ILocatableFolder folder, CancellationToken cancellationToken = default)
        {
            await Launcher.LaunchFolderPathAsync(folder.Path).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> PickSingleFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, MainWindow.Instance!.Hwnd);

            if (filter is not null)
            {
                filePicker.FileTypeFilter.EnumeratedAdd(filter);
            }
            else
            {
                filePicker.FileTypeFilter.Add("*");
            }

            var fileTask = filePicker.PickSingleFileAsync().AsTask(cancellationToken);
            var file = await fileTask;

            if (file is null)
                return null;

            return new WindowsStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder?> PickSingleFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Instance!.Hwnd);

            folderPicker.FileTypeFilter.Add("*");

            var folderTask = folderPicker.PickSingleFolderAsync().AsTask(cancellationToken);
            var folder = await folderTask;

            if (folder is null)
                return null;

            return new WindowsStorageFolder(folder);
        }
    }
}
