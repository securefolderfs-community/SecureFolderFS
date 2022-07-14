using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.WinUI.Storage.WindowsStorage;
using SecureFolderFS.WinUI.WindowViews;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class FileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task OpenAppFolderAsync()
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }

        /// <inheritdoc/>
        public async Task OpenInFileExplorerAsync(IFolder folder)
        {
            await Launcher.LaunchFolderPathAsync(folder.Path);
        }

        /// <inheritdoc/>
        public async Task<IFile?> PickSingleFileAsync(IEnumerable<string>? filter)
        {
            var filePicker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, MainWindow.Instance!.Hwnd);

            if (filter is not null)
                filePicker.FileTypeFilter.EnumeratedAdd(filter);
            else
            {
                filePicker.FileTypeFilter.Add("*");
            }

            var file = await filePicker.PickSingleFileAsync();
            if (file is null)
                return null;

            return new WindowsStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickSingleFolderAsync()
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Instance!.Hwnd);

            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is null)
                return null;

            return new WindowsStorageFolder(folder);
        }
    }
}
