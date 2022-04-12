using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.WinUI.WindowViews;

#nullable enable

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class FileExplorerService : IFileExplorerService
    {
        public async Task OpenPathInFileExplorerAsync(string path)
        {
            await Launcher.LaunchFolderPathAsync(path);
        }

        public async Task<string?> PickSingleFileAsync(IEnumerable<string>? filter)
        {
            var filePicker = new FileOpenPicker();

            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, MainWindow.Instance!.Hwnd);

            if (filter != null)
            {
                filePicker.FileTypeFilter.EnumeratedAdd(filter);
            }
            else
            {
                filePicker.FileTypeFilter.Add("*");
            }

            var file = await filePicker.PickSingleFileAsync();

            return file?.Path;
        }

        public async Task<string?> PickSingleFolderAsync()
        {
            var folderPicker = new FolderPicker();

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Instance!.Hwnd);

            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();

            return folder?.Path;
        }
    }
}
